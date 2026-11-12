// Copyright 2026 Niantic Spatial.

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using NianticSpatial.NSDK.AR.API;
using NianticSpatial.NSDK.AR.Core;
using NianticSpatial.NSDK.AR.Utilities;

namespace NianticSpatial.NSDK.AR.Subsystems.Playback
{
    /// <summary>
    /// Queries and edits the playback cache of the running NSDK session.
    /// </summary>
    /// <remarks>
    /// The cache exists while XR is initialized. Every call reads the current session, so one client
    /// keeps working across XR restarts, and throws <see cref="InvalidOperationException"/> while
    /// no session runs.
    /// </remarks>
    [PublicAPI]
    public class PlaybackCacheClient
    {
        private const int OpenPollIntervalMs = 100;

        private readonly IApi _api;
        private readonly Func<IntPtr> _getArdkHandle;

        /// <summary>
        /// Creates a client for the running NSDK session's playback cache.
        /// </summary>
        public PlaybackCacheClient() : this(new NativeApi(), GetContextArdkHandle)
        {
        }

        internal PlaybackCacheClient(IApi api, Func<IntPtr> getArdkHandle)
        {
            _api = api;
            _getArdkHandle = getArdkHandle;
        }

        /// <summary>
        /// Opens the scan's recording, downloading it into the cache unless it is already there.
        /// The caller closes the dataset.
        /// </summary>
        /// <remarks>
        /// Call from the main thread. A cached scan needs no network. Only the first chunk of a chunked
        /// recording plays.
        /// </remarks>
        /// <param name="scanId">The portal scan ID.</param>
        /// <param name="progress">Receives download progress in [0, 1].</param>
        /// <param name="cancellationToken">Cancels the download.</param>
        /// <exception cref="PlaybackOpenException">The open failed, including with
        /// <see cref="PlaybackOpenError.SessionEnded"/> when XR deinitializes before it finishes.</exception>
        /// <exception cref="OperationCanceledException"><paramref name="cancellationToken"/> was cancelled.</exception>
        /// <exception cref="InvalidOperationException">NSDK is not initialized, or the scan is already being opened.</exception>
        /// <exception cref="ArgumentException"><paramref name="scanId"/> is empty or not a valid scan ID.</exception>
        public async Task<PlaybackDataset> OpenAsync(
            string scanId, IProgress<float> progress = null, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var handle = GetArdkHandle();
            ulong requestId;
            using (var id = new ManagedNsdkString(scanId))
            {
                _api.RequestOpenFromScanId(handle, id.ToNsdkString(), out requestId).ThrowExceptionIfNeeded();
            }

            IntPtr datasetHandle;
            try
            {
                datasetHandle = await AwaitOpenAsync(handle, requestId, progress, cancellationToken);
            }
            catch (Exception e) when (!(e is PlaybackOpenException))
            {
                await CancelOpenAsync(handle, requestId);
                throw;
            }

            return PlaybackDataset.FromNativeHandle(_api, datasetHandle) ??
                throw new PlaybackOpenException(PlaybackOpenError.OpenFailed);
        }

        /// <summary>
        /// Whether the scan's recording is fully downloaded and extracted. Makes no network call.
        /// </summary>
        /// <param name="scanId">The scan to check.</param>
        /// <exception cref="ArgumentException"><paramref name="scanId"/> is empty or not a valid scan ID.</exception>
        public bool IsCached(string scanId)
        {
            var handle = GetArdkHandle();
            using var id = new ManagedNsdkString(scanId);
            _api.IsCached(handle, id.ToNsdkString(), out var isCached).ThrowExceptionIfNeeded();
            return isCached;
        }

        /// <summary>
        /// Lists the scans in the cache, in no particular order. Makes no network call.
        /// </summary>
        public IReadOnlyList<CachedPlayback> GetCachedPlaybacks()
        {
            var handle = GetArdkHandle();
            _api.GetCachedPlaybackList(handle, out var list).ThrowExceptionIfNeeded();
            try
            {
                var entries = new CachedPlayback[list.EntriesSize];
                var entrySize = Marshal.SizeOf<NsdkCachedPlayback>();
                for (var i = 0; i < entries.Length; i++)
                {
                    var native = Marshal.PtrToStructure<NsdkCachedPlayback>(IntPtr.Add(list.Entries, i * entrySize));
                    entries[i] = new CachedPlayback(native.ScanId.ToManagedString(), (CachedPlaybackState)native.State);
                }

                return entries;
            }
            finally
            {
                _api.ReleaseResource(list.Handle);
            }
        }

        /// <summary>
        /// Deletes the scan's cached recording. Close every dataset opened from the scan first.
        /// </summary>
        /// <param name="scanId">The scan whose recording to delete.</param>
        /// <exception cref="InvalidOperationException">The scan is being opened.</exception>
        /// <exception cref="ArgumentException"><paramref name="scanId"/> is empty or not a valid scan ID.</exception>
        public void DeleteCached(string scanId)
        {
            var handle = GetArdkHandle();
            using var id = new ManagedNsdkString(scanId);
            _api.DeleteCached(handle, id.ToNsdkString()).ThrowExceptionIfNeeded();
        }

        /// <summary>
        /// Deletes every cached recording. Close every dataset opened from the cache first.
        /// </summary>
        /// <exception cref="InvalidOperationException">A scan is being opened.</exception>
        public void ClearCache()
        {
            _api.ClearCache(GetArdkHandle()).ThrowExceptionIfNeeded();
        }

        /// <summary>Polls the request until it finishes, and returns its dataset handle.</summary>
        private async Task<IntPtr> AwaitOpenAsync(
            IntPtr handle, ulong requestId, IProgress<float> progress, CancellationToken cancellationToken)
        {
            float? reported = null;
            while (true)
            {
                if (!TryGetOpenResult(handle, requestId, out var result))
                {
                    throw new PlaybackOpenException(PlaybackOpenError.SessionEnded);
                }

                if (result.Progress != reported)
                {
                    reported = result.Progress;
                    try
                    {
                        progress?.Report(result.Progress);
                    }
                    catch when (result.Status == NsdkOpenFromScanIdStatus.Ready && result.Dataset.IsValidHandle())
                    {
                        // Native hands out a finished result once, so the cancel drain cannot reclaim it.
                        _api.Destroy(result.Dataset);
                        throw;
                    }
                }

                switch (result.Status)
                {
                    case NsdkOpenFromScanIdStatus.InProgress:
                        await Task.Delay(OpenPollIntervalMs, cancellationToken);
                        break;
                    case NsdkOpenFromScanIdStatus.Ready:
                        return result.Dataset.IsValidHandle()
                            ? result.Dataset
                            : throw new PlaybackOpenException(PlaybackOpenError.OpenFailed);
                    default:
                        if (Enum.IsDefined(typeof(PlaybackOpenError), result.Error))
                        {
                            throw new PlaybackOpenException((PlaybackOpenError)result.Error);
                        }

                        throw new InvalidOperationException(
                            $"Open-from-scan request {requestId} failed with unknown error {result.Error}");
                }
            }
        }

        /// <summary>Cancels the request and takes its result, destroying a dataset that beat the cancel.</summary>
        private async Task CancelOpenAsync(IntPtr handle, ulong requestId)
        {
            if (_getArdkHandle() != handle)
            {
                return;
            }

            // Fails when the request already finished.
            _api.CancelOpenFromScanId(handle, requestId);
            while (TryGetOpenResult(handle, requestId, out var result))
            {
                switch (result.Status)
                {
                    case NsdkOpenFromScanIdStatus.InProgress:
                        await Task.Delay(OpenPollIntervalMs);
                        break;
                    case NsdkOpenFromScanIdStatus.Ready:
                        if (result.Dataset.IsValidHandle())
                        {
                            _api.Destroy(result.Dataset);
                        }

                        return;
                    default:
                        return;
                }
            }
        }

        /// <summary>
        /// Reads the request's result; false once the session that owns it has ended.
        /// </summary>
        private bool TryGetOpenResult(IntPtr handle, ulong requestId, out NsdkOpenFromScanIdResult result)
        {
            result = default;
            if (_getArdkHandle() != handle)
            {
                return false;
            }

            var status = _api.GetOpenFromScanIdResult(handle, requestId, out result);
            // A restarted session reports the old request as unknown, and a destroyed cache as missing.
            if (status == NsdkStatus.InvalidArgument || status == NsdkStatus.FeatureDoesNotExist)
            {
                return false;
            }

            status.ThrowExceptionIfNeeded();
            return true;
        }

        private IntPtr GetArdkHandle()
        {
            var handle = _getArdkHandle();
            if (!handle.IsValidHandle())
            {
                throw new InvalidOperationException("NSDK is not initialized. Initialize XR before using the playback cache.");
            }

            return handle;
        }

        private static IntPtr GetContextArdkHandle()
        {
            var context = NsdkUnityContext.UnityContextHandle;
            return context.IsValidHandle() ? NsdkUnityContext.GetNSDKHandle(context) : IntPtr.Zero;
        }
    }
}
