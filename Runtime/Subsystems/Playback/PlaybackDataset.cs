// Copyright 2022-2026 Niantic Spatial.
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NianticSpatial.NSDK.AR.API;
using NianticSpatial.NSDK.AR.PAM;
using NianticSpatial.NSDK.AR.Utilities;
using NianticSpatial.NSDK.AR.Utilities.Logging;
using NianticSpatial.NSDK.Utilities.UnityAssets;
using UnityEngine;
using UnityEngine.XR.ARSubsystems;

namespace NianticSpatial.NSDK.AR.Subsystems.Playback
{
    /// <summary>
    /// A recorded playback dataset. The caller releases it with <see cref="Close"/> or <see cref="Dispose"/>.
    /// </summary>
    public class PlaybackDataset : IDisposable
    {
        private const string StreamingAssetsDirName = "StreamingAssets";

        private readonly IApi _api;
        private IntPtr _handle;
        private readonly FrameMetadata[] _frames;

        private PlaybackDataset(IApi api, IntPtr handle, NsdkPlaybackDatasetInfo info, string datasetPath)
        {
            _api = api;
            _handle = handle;
            DatasetPath = datasetPath;

            FrameCount = (int)info.FrameCount;
            FrameRate = (int)Math.Round(info.FrameRate);
            LidarEnabled = info.HasDepth;

            _frames = new FrameMetadata[FrameCount];
            NsdkClockBase? compassBase = null;
            for (var i = 0; i < FrameCount; i++)
            {
                var record = ReadFrame(i);
                _frames[i] = new FrameMetadata(record);
                if (i == 0)
                {
                    Resolution = new Vector2Int((int)record.ImageWidth, (int)record.ImageHeight);
                }

                if (record.HasDepth && DepthResolution == Vector2Int.zero)
                {
                    DepthResolution = new Vector2Int((int)record.DepthWidth, (int)record.DepthHeight);
                }

                LocationServicesEnabled |= record.Gps.TimestampMs != 0;
                if (record.Compass.TimestampMs != 0)
                {
                    CompassEnabled = true;
                    compassBase ??= record.Compass.TimestampBase;
                }
            }

            CompassTimestampBase = compassBase ?? NsdkClockBase.Epoch;
        }

        /// <summary>
        /// Opens the dataset at <paramref name="datasetPath"/> through native playback: a directory,
        /// relative to StreamingAssets or the persistent data path on device.
        /// </summary>
        /// <returns>The dataset, or null if it cannot be found or native playback cannot open it.</returns>
        internal static PlaybackDataset Open(string datasetPath, IApi api = null)
        {
            if (!TryResolvePath(datasetPath, out var path))
            {
                return null;
            }

            // Native playback reads frame images only.
            if (File.Exists(Path.Combine(path, "video.mp4")) &&
                !Directory.EnumerateFiles(path, "*.jpg", SearchOption.AllDirectories).Any())
            {
                Log.Error($"{path} holds video.mp4 without frame images; native playback reads frame images only.");
                return null;
            }

            api ??= new NativeApi();
            IntPtr handle;
            NsdkStatus status;
            using (var nativePath = new ManagedNsdkString(NativePath(path)))
            {
                status = api.Create(nativePath.ToNsdkString(), out handle);
            }

            if (!status.IsOk() || handle == IntPtr.Zero)
            {
                Log.Error($"Native playback could not open {path}: {status}");
                return null;
            }

            return FromNativeHandle(api, handle, path);
        }

        /// <summary>Wraps an open native playback <paramref name="handle"/> that has no dataset path.</summary>
        /// <returns>The dataset, or null after destroying the handle if native cannot read it.</returns>
        internal static PlaybackDataset FromNativeHandle(IApi api, IntPtr handle) =>
            FromNativeHandle(api, handle, null);

        private static PlaybackDataset FromNativeHandle(IApi api, IntPtr handle, string path)
        {
            var source = path ?? $"native dataset {handle}";
            var infoStatus = api.GetInfo(handle, out var info);
            if (!infoStatus.IsOk())
            {
                api.Destroy(handle);
                Log.Error($"Native playback could not read dataset info for {source}: {infoStatus}");
                return null;
            }

            PlaybackDataset dataset;
            try
            {
                dataset = new PlaybackDataset(api, handle, info, path);
            }
            catch (Exception e)
            {
                api.Destroy(handle);
                Log.Error($"Native playback could not read frame records for {source}: {e.Message}");
                return null;
            }

            return Validated(dataset, source);
        }

        internal string DatasetPath { get; }

        internal Vector2Int Resolution { get; }

        internal Vector2Int DepthResolution { get; }

        /// <summary>Number of frames in the dataset.</summary>
        public int FrameCount { get; }

        /// <summary>Recorded frame rate, in frames per second.</summary>
        public int FrameRate { get; }

        internal bool LocationServicesEnabled { get; }

        internal bool CompassEnabled { get; }

        internal bool LidarEnabled { get; }

        internal NsdkClockBase CompassTimestampBase { get; }

        internal IReadOnlyList<FrameMetadata> Frames => _frames;

        /// <summary>Releases native playback.</summary>
        public void Close()
        {
            if (!IsClosed)
            {
                _api.Destroy(_handle);
                _handle = IntPtr.Zero;
            }
        }

        /// <summary>Same as <see cref="Close"/>.</summary>
        public void Dispose() => Close();

        /// <summary>
        /// Opens the scan's recording through the running NSDK session's playback cache, and throws what
        /// <see cref="PlaybackCacheClient.OpenAsync"/> throws. The caller closes the dataset.
        /// </summary>
        /// <param name="scanId">The portal scan ID.</param>
        /// <param name="progress">Receives download progress in [0, 1].</param>
        /// <param name="cancellationToken">Cancels the download.</param>
        public static Task<PlaybackDataset> OpenAsync(
            string scanId, IProgress<float> progress = null, CancellationToken cancellationToken = default) =>
            new PlaybackCacheClient().OpenAsync(scanId, progress, cancellationToken);

        /// <summary>
        /// Deletes the scan's cached recording from the running NSDK session's playback cache. Close every
        /// dataset opened from the scan first.
        /// </summary>
        /// <param name="scanId">The scan whose recording to delete.</param>
        /// <exception cref="InvalidOperationException">NSDK is not initialized, or the scan is being opened.</exception>
        /// <exception cref="ArgumentException"><paramref name="scanId"/> is empty or not a valid scan ID.</exception>
        public static void DeleteCached(string scanId) => new PlaybackCacheClient().DeleteCached(scanId);

        /// <summary>
        /// Deletes every recording in the running NSDK session's playback cache. Close every dataset
        /// opened from the cache first.
        /// </summary>
        /// <exception cref="InvalidOperationException">NSDK is not initialized, or a scan is being opened.</exception>
        public static void ClearCache() => new PlaybackCacheClient().ClearCache();

        internal void Unload() => Close();

        internal bool IsClosed => _handle == IntPtr.Zero;

        /// <summary>Frame <paramref name="index"/>'s JPEG file bytes.</summary>
        internal byte[] ReadImage(int index) => ReadBuffers(index).Image;

        /// <summary>
        /// Frame <paramref name="index"/>'s image and depth bytes. Moves native's playback position to
        /// <paramref name="index"/>.
        /// </summary>
        internal PlaybackFrameBuffers ReadBuffers(int index)
        {
            var frameIndex = NativeFrameIndex(index);
            _api.SeekToFrame(_handle, frameIndex).ThrowExceptionIfNeeded();
            _api.GetCurrentFrame(_handle, out var frame, out var hasFrame).ThrowExceptionIfNeeded();
            return hasFrame ? PlaybackFrameBuffers.From(frame) : default;
        }

        private PlaybackFrameRecord ReadFrame(int index)
        {
            _api.GetFrameMetadata(_handle, NativeFrameIndex(index), out var frame).ThrowExceptionIfNeeded();
            return PlaybackFrameRecord.From(frame);
        }

        private uint NativeFrameIndex(int index)
        {
            if (IsClosed)
            {
                throw new ObjectDisposedException(nameof(PlaybackDataset));
            }

            if (index < 0 || index >= FrameCount)
            {
                throw new ArgumentOutOfRangeException(nameof(index),
                    $"Frame {index} is outside a dataset of {FrameCount} frames");
            }

            return (uint)index;
        }

        /// <summary>Resolves <paramref name="datasetPath"/> to a dataset directory.</summary>
        private static bool TryResolvePath(string datasetPath, out string resolved)
        {
            Log.Debug($"Attempting to load dataset from {datasetPath}");
            resolved = null;

            if (string.IsNullOrEmpty(datasetPath))
            {
                Log.Error($"Parameter '{nameof(datasetPath)}' is empty.");
                return false;
            }

            // If run through the Unity Editor, FileUtilities is able to handle any absolute or project relative path.
            // We need to create the absolute path on-device.
            if (!Application.isEditor)
            {
                if (datasetPath.Contains(StreamingAssetsDirName))
                {
                    datasetPath = GetAbsoluteStreamingAssetsPath(datasetPath);
                    Log.Info($"Attempting to load dataset from StreamingAssets directory with absolute path {datasetPath}");
                }
                else
                {
                    datasetPath = Path.Combine(Application.persistentDataPath, datasetPath);
                    Log.Info($"Attempting to load dataset from persistent data directory with absolute path {datasetPath}");
                }
            }

            if (!string.IsNullOrEmpty(Path.GetExtension(datasetPath)))
            {
                Log.Error($"Parameter '{nameof(datasetPath)}' must be a dataset directory.");
                return false;
            }

            resolved = datasetPath;
            return true;
        }

        /// <summary>Rejects empty datasets and warns when the recording's orientation differs from the display's.</summary>
        private static PlaybackDataset Validated(PlaybackDataset dataset, string datasetPath)
        {
            if (dataset.FrameCount == 0)
            {
                Log.Error($"Loaded dataset from {datasetPath} but found 0 frames.");
                dataset.Close();
                return null;
            }

            var datasetOrientation = dataset.Frames[0].Orientation;
            var liveOrientation =
                Application.isEditor ? GameViewUtils.GetEditorScreenOrientation() : Screen.orientation;

            if (datasetOrientation != liveOrientation)
            {
                Log.Warning
                (
                    $"The dataset was recorded in {datasetOrientation} orientation but the game display " +
                    $"is currently in {liveOrientation}. This may result in visual discrepancies."
                );
            }

            Log.Info($"Successfully loaded dataset with {dataset.FrameCount} frames from {datasetPath}");
            return dataset;
        }

        // Android packs StreamingAssets into the APK as jar:file://…!/assets/<path>; native reads APK
        // assets through its asset:// scheme instead.
        private static string NativePath(string datasetPath)
        {
            const string apkAssetsMarker = "!/assets/";
            var marker = datasetPath.IndexOf(apkAssetsMarker, StringComparison.Ordinal);
            return datasetPath.StartsWith("jar:", StringComparison.Ordinal) && marker >= 0
                ? "asset://" + datasetPath.Substring(marker + apkAssetsMarker.Length)
                : datasetPath;
        }

        private static string GetAbsoluteStreamingAssetsPath(string path)
        {
            var relativePath = path.Split(StreamingAssetsDirName + Path.DirectorySeparatorChar).Last();
            return Path.Combine(Application.streamingAssetsPath, relativePath);
        }

        internal class FrameMetadata
        {
            private readonly Matrix4x4 _recordedPose;
            private readonly CameraIntrinsicsCStruct _intrinsics;
            private readonly Vector2Int _resolution;
            private readonly LocationInfo _location;

            internal FrameMetadata(PlaybackFrameRecord record)
            {
                Sequence = (int)record.Sequence;
                HasDepth = record.HasDepth;
                _recordedPose = RecordedPose(record.CameraPose);
                _intrinsics = record.Intrinsics;
                _resolution = new Vector2Int((int)record.ImageWidth, (int)record.ImageHeight);
                // capture.json records ARKit's NotAvailable, Limited, Normal; native adds one, keeping 0 as Unknown.
                TrackingState = record.TrackingState switch
                {
                    3 => TrackingState.Tracking,
                    2 => TrackingState.Limited,
                    _ => TrackingState.None,
                };
                TimestampInSeconds = record.TimestampMs / 1000.0;
                _location = new LocationInfo(record.Gps, record.Compass);
            }

            public FrameMetadata(FrameMetadata original, double timestampOffset)
            {
                Sequence = original.Sequence;
                HasDepth = original.HasDepth;
                _recordedPose = original._recordedPose;
                _intrinsics = original._intrinsics;
                _resolution = original._resolution;
                TrackingState = original.TrackingState;
                TimestampInSeconds = original.TimestampInSeconds + timestampOffset;
                _location = new LocationInfo(original._location, timestampOffset);
            }

            public int Sequence { get; }

            public bool HasDepth { get; }

            public LocationInfo LocationInfo => _location.PositionTimestamp == 0 ? null : _location;

            public Matrix4x4 Pose => _recordedPose.FromOpenGLToUnity();

            public ScreenOrientation Orientation => Pose.GetScreenOrientation();

            public XRCameraIntrinsics Intrinsics =>
                new XRCameraIntrinsics
                (
                    new Vector2(_intrinsics.FocalLengthX, _intrinsics.FocalLengthY),
                    new Vector2(_intrinsics.PrincipalPointX, _intrinsics.PrincipalPointY),
                    _resolution
                );

            public TrackingState TrackingState { get; }

            public double TimestampInSeconds { get; }

            // Native stores F·P·F with F = diag(1, -1, -1, 1); undo it to get the recorded camera-to-world.
            private static Matrix4x4 RecordedPose(TransformCStruct pose)
            {
                var nsdk = Matrix4x4.TRS(
                    new Vector3(pose.TranslationX, pose.TranslationY, pose.TranslationZ),
                    new Quaternion(pose.OrientationX, pose.OrientationY, pose.OrientationZ, pose.OrientationW),
                    Vector3.one);
                var flip = Matrix4x4.Scale(new Vector3(1, -1, -1));
                return flip * nsdk * flip;
            }
        }

        internal class LocationInfo
        {
            internal LocationInfo(GpsLocationCStruct gps, CompassDataCStruct compass)
            {
                Latitude = gps.Latitude;
                Longitude = gps.Longitude;
                PositionAccuracy = gps.HorizontalAccuracy;
                PositionTimestamp = gps.TimestampMs / 1000.0;
                Altitude = gps.Altitude;
                AltitudeAccuracy = gps.VerticalAccuracy;
                Heading = compass.TrueHeading;
                HeadingAccuracy = compass.HeadingAccuracy;
                HeadingTimestamp = compass.TimestampMs / 1000.0;
                MagnetometerCalibrated = compass.TimestampMs == 0
                    ? null
                    : new double[] { compass.RawDataX, compass.RawDataY, compass.RawDataZ };
            }

            public LocationInfo(LocationInfo original, double timestampOffset)
            {
                Latitude = original.Latitude;
                Longitude = original.Longitude;
                PositionAccuracy = original.PositionAccuracy;
                // A zero timestamp marks no recorded reading; offsetting it would make one appear.
                PositionTimestamp = original.PositionTimestamp == 0 ? 0 : original.PositionTimestamp + timestampOffset;
                Altitude = original.Altitude;
                AltitudeAccuracy = original.AltitudeAccuracy;
                Heading = original.Heading;
                HeadingAccuracy = original.HeadingAccuracy;
                HeadingTimestamp = original.HeadingTimestamp == 0 ? 0 : original.HeadingTimestamp + timestampOffset;
                MagnetometerCalibrated = original.MagnetometerCalibrated;
            }

            public double Latitude { get; }

            public double Longitude { get; }

            public float PositionAccuracy { get; }

            public double PositionTimestamp { get; }

            public double Altitude { get; }

            public double AltitudeAccuracy { get; }

            public float Heading { get; }

            public float HeadingAccuracy { get; }

            public double HeadingTimestamp { get; }

            public double[] MagnetometerCalibrated { get; }
        }
    }
}
