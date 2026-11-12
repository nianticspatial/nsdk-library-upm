// Copyright 2026 Niantic Spatial.

using System;
using System.Runtime.InteropServices;
using NianticSpatial.NSDK.AR.API;
using NianticSpatial.NSDK.AR.Core;
using NianticSpatial.NSDK.AR.PAM;

namespace NianticSpatial.NSDK.AR.Subsystems.Playback
{
    // Mirrors the native ARDK_PlaybackDataset_Info struct.
    [StructLayout(LayoutKind.Sequential)]
    internal struct NsdkPlaybackDatasetInfo
    {
        public uint FrameCount;
        public double FrameRate;
        private byte _hasDepth; // C bool is 1 byte

        public bool HasDepth => _hasDepth != 0;
    }

    // Mirrors the native ARDK_PlaybackDataset_CachedPlayback struct.
    [StructLayout(LayoutKind.Sequential)]
    internal struct NsdkCachedPlayback
    {
        public NsdkString ScanId;
        public int State;
    }

    // Mirrors the native ARDK_PlaybackDataset_CachedPlaybackList struct.
    [StructLayout(LayoutKind.Sequential)]
    internal struct NsdkCachedPlaybackList
    {
        public IntPtr Handle;
        public IntPtr Entries;
        public int EntriesSize;
    }

    // Mirrors the native ARDK_PlaybackDataset_OpenFromScanIdStatus enum.
    internal enum NsdkOpenFromScanIdStatus
    {
        InProgress = 0,
        Ready,
        Failed,
        Cancelled
    }

    // Mirrors the native ARDK_PlaybackDataset_OpenFromScanIdResult struct.
    [StructLayout(LayoutKind.Sequential)]
    internal struct NsdkOpenFromScanIdResult
    {
        public NsdkOpenFromScanIdStatus Status;
        public float Progress;
        public int Error;
        public IntPtr Dataset;
    }

    /// <summary>
    /// Default native-backed implementation of <see cref="IApi"/> using P/Invoke.
    /// </summary>
    internal class NativeApi : IApi
    {
        // Native looping stays off: PlaybackDatasetReader seeks by frame index and loops itself.
        public NsdkStatus Create(NsdkString datasetPath, out IntPtr datasetHandle) =>
            (NsdkStatus)Native.Create(datasetPath, loopPlayback: false, out datasetHandle);

        public NsdkStatus Destroy(IntPtr datasetHandle) => (NsdkStatus)Native.Destroy(datasetHandle);

        public NsdkStatus GetInfo(IntPtr datasetHandle, out NsdkPlaybackDatasetInfo info) =>
            (NsdkStatus)Native.GetInfo(datasetHandle, out info);

        public NsdkStatus SeekToFrame(IntPtr datasetHandle, uint frameIndex) =>
            (NsdkStatus)Native.SeekToFrame(datasetHandle, frameIndex);

        public NsdkStatus GetCurrentFrame(IntPtr datasetHandle, out NsdkFrameData frame, out bool hasFrame)
        {
            var status = (NsdkStatus)Native.GetCurrentFrame(datasetHandle, out frame, out byte hasFrameByte);
            hasFrame = hasFrameByte != 0;
            return status;
        }

        public NsdkStatus GetFrameMetadata(IntPtr datasetHandle, uint frameIndex, out NsdkFrameData frame) =>
            (NsdkStatus)Native.GetFrameMetadata(datasetHandle, frameIndex, out frame);

        public NsdkStatus CreateCache(IntPtr ardkHandle) => (NsdkStatus)Native.CreateCache(ardkHandle);

        public NsdkStatus DestroyCache(IntPtr ardkHandle) => (NsdkStatus)Native.DestroyCache(ardkHandle);

        public NsdkStatus IsCached(IntPtr ardkHandle, NsdkString scanId, out bool isCached)
        {
            var status = (NsdkStatus)Native.IsCached(ardkHandle, scanId, out byte isCachedByte);
            isCached = isCachedByte != 0;
            return status;
        }

        public NsdkStatus GetCachedPlaybackList(IntPtr ardkHandle, out NsdkCachedPlaybackList list) =>
            (NsdkStatus)Native.GetCachedPlaybackList(ardkHandle, out list);

        public NsdkStatus DeleteCached(IntPtr ardkHandle, NsdkString scanId) =>
            (NsdkStatus)Native.DeleteCached(ardkHandle, scanId);

        public NsdkStatus ClearCache(IntPtr ardkHandle) => (NsdkStatus)Native.ClearCache(ardkHandle);

        public NsdkStatus RequestOpenFromScanId(IntPtr ardkHandle, NsdkString scanId, out ulong requestId) =>
            (NsdkStatus)Native.RequestOpenFromScanId(ardkHandle, scanId, out requestId);

        public NsdkStatus GetOpenFromScanIdResult(
            IntPtr ardkHandle, ulong requestId, out NsdkOpenFromScanIdResult result) =>
            (NsdkStatus)Native.GetOpenFromScanIdResult(ardkHandle, requestId, out result);

        public NsdkStatus CancelOpenFromScanId(IntPtr ardkHandle, ulong requestId) =>
            (NsdkStatus)Native.CancelOpenFromScanId(ardkHandle, requestId);

        public void ReleaseResource(IntPtr resourceHandle) => NsdkExternUtils.ReleaseResource(resourceHandle);

        private static class Native
        {
            [DllImport(NsdkPlugin.Name, EntryPoint = "ARDK_PlaybackDataset_Create")]
            public static extern int Create(
                NsdkString datasetPath, [MarshalAs(UnmanagedType.I1)] bool loopPlayback, out IntPtr datasetHandle);

            [DllImport(NsdkPlugin.Name, EntryPoint = "ARDK_PlaybackDataset_Destroy")]
            public static extern int Destroy(IntPtr datasetHandle);

            [DllImport(NsdkPlugin.Name, EntryPoint = "ARDK_PlaybackDataset_GetInfo")]
            public static extern int GetInfo(IntPtr datasetHandle, out NsdkPlaybackDatasetInfo info);

            [DllImport(NsdkPlugin.Name, EntryPoint = "ARDK_PlaybackDataset_SeekToFrame")]
            public static extern int SeekToFrame(IntPtr datasetHandle, uint frameIndex);

            [DllImport(NsdkPlugin.Name, EntryPoint = "ARDK_PlaybackDataset_GetCurrentFrame")]
            public static extern int GetCurrentFrame(IntPtr datasetHandle, out NsdkFrameData frame, out byte hasFrame);

            [DllImport(NsdkPlugin.Name, EntryPoint = "ARDK_PlaybackDataset_GetFrameMetadata")]
            public static extern int GetFrameMetadata(IntPtr datasetHandle, uint frameIndex, out NsdkFrameData frame);

            [DllImport(NsdkPlugin.Name, EntryPoint = "ARDK_PlaybackCache_Create")]
            public static extern int CreateCache(IntPtr ardkHandle);

            [DllImport(NsdkPlugin.Name, EntryPoint = "ARDK_PlaybackCache_Destroy")]
            public static extern int DestroyCache(IntPtr ardkHandle);

            [DllImport(NsdkPlugin.Name, EntryPoint = "ARDK_PlaybackDataset_IsCachedPlayback")]
            public static extern int IsCached(IntPtr ardkHandle, NsdkString scanId, out byte isCached);

            [DllImport(NsdkPlugin.Name, EntryPoint = "ARDK_PlaybackDataset_GetCachedPlaybackList")]
            public static extern int GetCachedPlaybackList(IntPtr ardkHandle, out NsdkCachedPlaybackList list);

            [DllImport(NsdkPlugin.Name, EntryPoint = "ARDK_PlaybackDataset_DeleteCachedPlayback")]
            public static extern int DeleteCached(IntPtr ardkHandle, NsdkString scanId);

            [DllImport(NsdkPlugin.Name, EntryPoint = "ARDK_PlaybackDataset_ClearPlaybackCache")]
            public static extern int ClearCache(IntPtr ardkHandle);

            [DllImport(NsdkPlugin.Name, EntryPoint = "ARDK_PlaybackDataset_RequestOpenFromScanId")]
            public static extern int RequestOpenFromScanId(IntPtr ardkHandle, NsdkString scanId, out ulong requestId);

            [DllImport(NsdkPlugin.Name, EntryPoint = "ARDK_PlaybackDataset_GetOpenFromScanIdResult")]
            public static extern int GetOpenFromScanIdResult(
                IntPtr ardkHandle, ulong requestId, out NsdkOpenFromScanIdResult result);

            [DllImport(NsdkPlugin.Name, EntryPoint = "ARDK_PlaybackDataset_CancelOpenFromScanId")]
            public static extern int CancelOpenFromScanId(IntPtr ardkHandle, ulong requestId);
        }
    }
}
