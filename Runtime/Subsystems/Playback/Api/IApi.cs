// Copyright 2026 Niantic Spatial.

using System;
using NianticSpatial.NSDK.AR.API;
using NianticSpatial.NSDK.AR.PAM;

namespace NianticSpatial.NSDK.AR.Subsystems.Playback
{
    /// <summary>
    /// Wraps the native playback dataset and playback cache C API. Implementations:
    /// <see cref="NativeApi"/> (real P/Invoke) and test doubles.
    /// </summary>
    internal interface IApi
    {
        NsdkStatus Create(NsdkString datasetPath, out IntPtr datasetHandle);
        NsdkStatus Destroy(IntPtr datasetHandle);
        NsdkStatus GetInfo(IntPtr datasetHandle, out NsdkPlaybackDatasetInfo info);
        NsdkStatus SeekToFrame(IntPtr datasetHandle, uint frameIndex);
        NsdkStatus GetCurrentFrame(IntPtr datasetHandle, out NsdkFrameData frame, out bool hasFrame);
        NsdkStatus GetFrameMetadata(IntPtr datasetHandle, uint frameIndex, out NsdkFrameData frame);

        NsdkStatus CreateCache(IntPtr ardkHandle);
        NsdkStatus DestroyCache(IntPtr ardkHandle);
        NsdkStatus IsCached(IntPtr ardkHandle, NsdkString scanId, out bool isCached);
        NsdkStatus GetCachedPlaybackList(IntPtr ardkHandle, out NsdkCachedPlaybackList list);
        NsdkStatus DeleteCached(IntPtr ardkHandle, NsdkString scanId);
        NsdkStatus ClearCache(IntPtr ardkHandle);
        NsdkStatus RequestOpenFromScanId(IntPtr ardkHandle, NsdkString scanId, out ulong requestId);
        NsdkStatus GetOpenFromScanIdResult(IntPtr ardkHandle, ulong requestId, out NsdkOpenFromScanIdResult result);
        NsdkStatus CancelOpenFromScanId(IntPtr ardkHandle, ulong requestId);
        void ReleaseResource(IntPtr resourceHandle);
    }
}
