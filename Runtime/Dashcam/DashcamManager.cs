// Copyright 2022-2026 Niantic Spatial.

using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using NianticSpatial.NSDK.AR.API;
using NianticSpatial.NSDK.AR.Core;
using NianticSpatial.NSDK.AR.Subsystems.Dashcam;
using NianticSpatial.NSDK.AR.Utilities.Logging;
using UnityEngine;
using IApi = NianticSpatial.NSDK.AR.Subsystems.Dashcam.IApi;
using NativeApi = NianticSpatial.NSDK.AR.Subsystems.Dashcam.NativeApi;

namespace NianticSpatial.NSDK.AR.Dashcam
{
    /// <summary>
    /// MonoBehaviour that manages the NSDK dashcam feature lifecycle.
    /// Attach to a GameObject in your scene to enable continuous frame buffering
    /// with the ability to save the buffer to a V2 sequence on demand.
    /// </summary>
    public class DashcamManager : MonoBehaviour
    {
        [Header("Configuration")]
        [Tooltip("Target FPS for buffering (0 = native default of 5).")]
        [SerializeField] private int _framerate = 5;

        [Tooltip("Maximum seconds of data to buffer (0 = native default of 60).")]
        [SerializeField] private int _maxBufferSeconds = 60;

        [Tooltip("Maximum buffer memory in MB (0 = native default of 128).")]
        [SerializeField] private int _maxBufferMemoryMb = 128;

        [Tooltip("Base path for saving. Leave empty for platform default.")]
        [SerializeField] private string _basePath = "";

        private IApi _api;
        private IntPtr _nsdkHandle = IntPtr.Zero;
        private bool _isRunning;

        /// <summary>
        /// Whether the dashcam is currently buffering frames.
        /// </summary>
        public bool IsRunning => _isRunning;

        private void OnEnable()
        {
            _api ??= new NativeApi();
            _nsdkHandle = NsdkUnityContext.GetNSDKHandle(NsdkUnityContext.UnityContextHandle);
            if (!_nsdkHandle.IsValidHandle())
            {
                Log.Error("DashcamManager: NSDK handle is not available. " +
                    "Ensure NSDK is initialized before enabling DashcamManager.");
                return;
            }

            _api.Create(_nsdkHandle).ThrowExceptionIfNeeded();
            ApplyConfiguration();
        }

        private void OnDisable()
        {
            if (!_nsdkHandle.IsValidHandle())
                return;

            if (_isRunning)
                StopBuffering();

            _api.Destroy(_nsdkHandle);
            _nsdkHandle = IntPtr.Zero;
        }

        /// <summary>
        /// Apply the current configuration to the native dashcam.
        /// Must be called while the dashcam is stopped.
        /// </summary>
        public void ApplyConfiguration()
        {
            if (!_nsdkHandle.IsValidHandle())
            {
                Log.Warning("DashcamManager: Cannot configure before creation.");
                return;
            }

            if (_isRunning)
            {
                Log.Warning("DashcamManager: Cannot reconfigure while running. Stop first.");
                return;
            }

            using var managedBasePath = new ManagedNsdkString(_basePath ?? string.Empty);
            var config = new NsdkDashcamConfig
            {
                Framerate = _framerate,
                MaxBufferSeconds = _maxBufferSeconds,
                MaxBufferMemoryMb = _maxBufferMemoryMb,
                BasePath = managedBasePath.ToNsdkString(),
            };
            _api.Configure(_nsdkHandle, ref config).ThrowExceptionIfNeeded();
        }

        /// <summary>
        /// Update configuration values from script. Call <see cref="ApplyConfiguration"/>
        /// afterwards (while stopped) to push them to native.
        /// </summary>
        public void SetConfiguration(DashcamConfiguration config)
        {
            _framerate = config.Framerate;
            _maxBufferSeconds = config.MaxBufferSeconds;
            _maxBufferMemoryMb = config.MaxBufferMemoryMb;
            _basePath = config.BasePath ?? string.Empty;
        }

        /// <summary>
        /// Start buffering frames.
        /// </summary>
        public void StartBuffering()
        {
            if (!_nsdkHandle.IsValidHandle())
            {
                Log.Warning("DashcamManager: Not created.");
                return;
            }

            if (_isRunning)
            {
                Log.Warning("DashcamManager: Already running.");
                return;
            }

            _api.Start(_nsdkHandle).ThrowExceptionIfNeeded();
            _isRunning = true;
        }

        /// <summary>
        /// Stop buffering frames. Buffer is cleared.
        /// Saving is not allowed while stopped.
        /// </summary>
        public void StopBuffering()
        {
            if (!_isRunning)
                return;

            _api.Stop(_nsdkHandle).ThrowExceptionIfNeeded();
            _isRunning = false;
        }

        /// <summary>
        /// Get the current buffer state.
        /// </summary>
        public DashcamBufferInfo GetBufferInfo()
        {
            var info = new DashcamBufferInfo();
            if (!_nsdkHandle.IsValidHandle())
                return info;

            _api.GetBufferInfo(_nsdkHandle, out var native).ThrowExceptionIfNeeded();
            info.FrameCount = native.FrameCount;
            info.MemoryUsedBytes = native.MemoryUsedBytes;
            info.DurationMs = native.DurationMs;
            return info;
        }

        /// <summary>
        /// Trigger a save of the current buffer to disk as a V2 sequence and
        /// await the result. The dashcam continues buffering during the save.
        /// </summary>
        public async Task<DashcamSaveResult> TriggerSaveAsync()
        {
            if (!_nsdkHandle.IsValidHandle())
            {
                Log.Warning("DashcamManager: Not created.");
                return new DashcamSaveResult { State = DashcamSaveState.NotAvailable };
            }

            _api.TriggerSave(_nsdkHandle).ThrowExceptionIfNeeded();

            while (true)
            {
                await Task.Yield();
                if (TryReadSaveResult(out var result))
                    return result;
            }
        }

        private bool TryReadSaveResult(out DashcamSaveResult result)
        {
            result = new DashcamSaveResult { State = DashcamSaveState.NotAvailable };
            if (!_nsdkHandle.IsValidHandle())
                return false;

            var status = _api.GetSaveResult(_nsdkHandle, out var native);
            if (status != NsdkStatus.Ok)
                return false;

            var state = (DashcamSaveState)(int)native.State;
            if (state == DashcamSaveState.NotAvailable)
                return false;

            result.State = state;
            if (native.SavePath != IntPtr.Zero && native.SavePathLen > 0)
                result.SavePath = Marshal.PtrToStringAnsi(native.SavePath, (int)native.SavePathLen);
            if (native.ArchivePath != IntPtr.Zero && native.ArchivePathLen > 0)
                result.ArchivePath =
                    Marshal.PtrToStringAnsi(native.ArchivePath, (int)native.ArchivePathLen);

            if (native.Handle != IntPtr.Zero)
                NsdkExternUtils.ReleaseResource(native.Handle);
            return true;
        }
    }
}
