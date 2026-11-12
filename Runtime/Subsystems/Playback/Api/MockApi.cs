// Copyright 2026 Niantic Spatial.

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using NianticSpatial.NSDK.AR.API;
using NianticSpatial.NSDK.AR.PAM;

namespace NianticSpatial.NSDK.AR.Subsystems.Playback
{
    /// <summary>Scripted <see cref="IApi"/> for tests; frames carry camera data only when <see cref="FramesHaveCamera"/> is set.</summary>
    internal class MockApi : IApi
    {
        public NsdkStatus CreateStatus { get; set; } = NsdkStatus.Ok;
        public IntPtr Handle { get; set; } = new IntPtr(1);
        public NsdkStatus InfoStatus { get; set; } = NsdkStatus.Ok;
        public uint FrameCount { get; set; } = 1;
        public NsdkStatus FrameMetadataStatus { get; set; } = NsdkStatus.Ok;
        public bool FramesHaveCamera { get; set; }

        public NsdkStatus IsCachedStatus { get; set; } = NsdkStatus.Ok;
        public bool IsCachedResult { get; set; }
        public NsdkStatus ListStatus { get; set; } = NsdkStatus.Ok;
        public IntPtr ListHandle { get; set; } = new IntPtr(2);
        public NsdkStatus DeleteStatus { get; set; } = NsdkStatus.Ok;
        public NsdkStatus ClearStatus { get; set; } = NsdkStatus.Ok;
        public NsdkStatus RequestOpenStatus { get; set; } = NsdkStatus.Ok;
        public ulong OpenRequestId { get; set; } = 3;
        public NsdkStatus OpenResultStatus { get; set; } = NsdkStatus.Ok;
        public NsdkStatus CancelStatus { get; set; } = NsdkStatus.Ok;

        /// <summary>
        /// Results that successive polls of one open request return. A last in-progress entry repeats;
        /// after a finished entry, polls return <see cref="NsdkStatus.InvalidArgument"/>.
        /// </summary>
        public List<NsdkOpenFromScanIdResult> OpenResults { get; } = new List<NsdkOpenFromScanIdResult>();

        /// <summary>Poll results for the scans listed here, in place of <see cref="OpenResults"/>.</summary>
        public Dictionary<string, List<NsdkOpenFromScanIdResult>> OpenResultsByScanId { get; } =
            new Dictionary<string, List<NsdkOpenFromScanIdResult>>();

        /// <summary>Whether a cancelled request's next poll returns a cancelled result.</summary>
        public bool CancelEndsOpen { get; set; }

        /// <summary>Entries the next list call returns, as scan ID and native state.</summary>
        public List<(string ScanId, int State)> CachedEntries { get; } = new List<(string, int)>();

        public List<IntPtr> DestroyedHandles { get; } = new List<IntPtr>();
        public List<IntPtr> ArdkHandles { get; } = new List<IntPtr>();
        public List<string> IsCachedScanIds { get; } = new List<string>();
        public List<string> DeletedScanIds { get; } = new List<string>();
        public List<IntPtr> ReleasedResources { get; } = new List<IntPtr>();
        public int ClearCount { get; private set; }
        public List<string> OpenScanIds { get; } = new List<string>();
        public List<ulong> CancelledRequestIds { get; } = new List<ulong>();
        public int OpenPollCount { get; private set; }

        // Open requests by request ID; the first gets OpenRequestId, later ones count up from it.
        private readonly Dictionary<ulong, OpenRequest> _openRequests = new Dictionary<ulong, OpenRequest>();

        // Unmanaged entries and strings behind the last list; freed on release.
        private readonly List<IntPtr> _listAllocations = new List<IntPtr>();

        // Identity-pose camera frame header shared by every mock frame; lives for the process.
        private static readonly IntPtr CameraFrame = AllocCameraFrame();

        public NsdkStatus Create(NsdkString datasetPath, out IntPtr datasetHandle)
        {
            datasetHandle = CreateStatus == NsdkStatus.Ok ? Handle : IntPtr.Zero;
            return CreateStatus;
        }

        public NsdkStatus Destroy(IntPtr datasetHandle)
        {
            DestroyedHandles.Add(datasetHandle);
            return NsdkStatus.Ok;
        }

        public NsdkStatus GetInfo(IntPtr datasetHandle, out NsdkPlaybackDatasetInfo info)
        {
            info = new NsdkPlaybackDatasetInfo { FrameCount = FrameCount, FrameRate = 30 };
            return InfoStatus;
        }

        public NsdkStatus SeekToFrame(IntPtr datasetHandle, uint frameIndex) => NsdkStatus.Ok;

        public NsdkStatus GetCurrentFrame(IntPtr datasetHandle, out NsdkFrameData frame, out bool hasFrame)
        {
            frame = default;
            hasFrame = false;
            return NsdkStatus.Ok;
        }

        public NsdkStatus GetFrameMetadata(IntPtr datasetHandle, uint frameIndex, out NsdkFrameData frame)
        {
            frame = FramesHaveCamera
                ? new NsdkFrameData { FrameId = frameIndex, CameraFrames = CameraFrame, CameraFramesCount = 1 }
                : default;
            return FrameMetadataStatus;
        }

        public NsdkStatus CreateCache(IntPtr ardkHandle) => NsdkStatus.Ok;

        public NsdkStatus DestroyCache(IntPtr ardkHandle) => NsdkStatus.Ok;

        public NsdkStatus IsCached(IntPtr ardkHandle, NsdkString scanId, out bool isCached)
        {
            ArdkHandles.Add(ardkHandle);
            IsCachedScanIds.Add(scanId.ToManagedString());
            isCached = IsCachedStatus == NsdkStatus.Ok && IsCachedResult;
            return IsCachedStatus;
        }

        public NsdkStatus GetCachedPlaybackList(IntPtr ardkHandle, out NsdkCachedPlaybackList list)
        {
            ArdkHandles.Add(ardkHandle);
            list = default;
            if (ListStatus != NsdkStatus.Ok)
            {
                return ListStatus;
            }

            list.Handle = ListHandle;
            list.EntriesSize = CachedEntries.Count;
            if (CachedEntries.Count == 0)
            {
                return ListStatus;
            }

            var entrySize = Marshal.SizeOf<NsdkCachedPlayback>();
            list.Entries = Marshal.AllocHGlobal(entrySize * CachedEntries.Count);
            _listAllocations.Add(list.Entries);
            for (var i = 0; i < CachedEntries.Count; i++)
            {
                var bytes = Encoding.UTF8.GetBytes(CachedEntries[i].ScanId);
                var data = Marshal.AllocHGlobal(bytes.Length);
                _listAllocations.Add(data);
                Marshal.Copy(bytes, 0, data, bytes.Length);
                var entry = new NsdkCachedPlayback
                {
                    ScanId = new NsdkString { data = data, length = (uint)bytes.Length },
                    State = CachedEntries[i].State,
                };
                Marshal.StructureToPtr(entry, IntPtr.Add(list.Entries, i * entrySize), false);
            }

            return ListStatus;
        }

        public NsdkStatus DeleteCached(IntPtr ardkHandle, NsdkString scanId)
        {
            ArdkHandles.Add(ardkHandle);
            DeletedScanIds.Add(scanId.ToManagedString());
            return DeleteStatus;
        }

        public NsdkStatus ClearCache(IntPtr ardkHandle)
        {
            ArdkHandles.Add(ardkHandle);
            ClearCount++;
            return ClearStatus;
        }

        public NsdkStatus RequestOpenFromScanId(IntPtr ardkHandle, NsdkString scanId, out ulong requestId)
        {
            ArdkHandles.Add(ardkHandle);
            var id = scanId.ToManagedString();
            OpenScanIds.Add(id);
            requestId = 0;
            if (RequestOpenStatus != NsdkStatus.Ok)
            {
                return RequestOpenStatus;
            }

            requestId = OpenRequestId + (ulong)_openRequests.Count;
            _openRequests[requestId] = new OpenRequest
            {
                Results = OpenResultsByScanId.TryGetValue(id, out var results) ? results : OpenResults
            };
            return RequestOpenStatus;
        }

        public NsdkStatus GetOpenFromScanIdResult(
            IntPtr ardkHandle, ulong requestId, out NsdkOpenFromScanIdResult result)
        {
            ArdkHandles.Add(ardkHandle);
            OpenPollCount++;
            if (!_openRequests.TryGetValue(requestId, out var request) || request.Taken)
            {
                result = default;
                return NsdkStatus.InvalidArgument;
            }

            request.PollCount++;
            if (request.Cancelled && CancelEndsOpen)
            {
                result = new NsdkOpenFromScanIdResult
                {
                    Status = NsdkOpenFromScanIdStatus.Cancelled, Error = (int)PlaybackOpenError.Cancelled
                };
            }
            else
            {
                var results = request.Results;
                result = results.Count == 0 ? default : results[Math.Min(request.PollCount - 1, results.Count - 1)];
            }

            request.Taken = OpenResultStatus == NsdkStatus.Ok && result.Status != NsdkOpenFromScanIdStatus.InProgress;
            return OpenResultStatus;
        }

        public NsdkStatus CancelOpenFromScanId(IntPtr ardkHandle, ulong requestId)
        {
            ArdkHandles.Add(ardkHandle);
            CancelledRequestIds.Add(requestId);
            if (CancelStatus == NsdkStatus.Ok && _openRequests.TryGetValue(requestId, out var request))
            {
                request.Cancelled = true;
            }

            return CancelStatus;
        }

        public void ReleaseResource(IntPtr resourceHandle)
        {
            ReleasedResources.Add(resourceHandle);
            foreach (var allocation in _listAllocations)
            {
                Marshal.FreeHGlobal(allocation);
            }

            _listAllocations.Clear();
        }

        private class OpenRequest
        {
            public List<NsdkOpenFromScanIdResult> Results;
            public int PollCount;
            public bool Taken;
            public bool Cancelled;
        }

        private static IntPtr AllocCameraFrame()
        {
            var camera = new NsdkCameraFrameCStruct { CameraPose = new TransformCStruct { OrientationW = 1 } };
            var pointer = Marshal.AllocHGlobal(Marshal.SizeOf<NsdkCameraFrameCStruct>());
            Marshal.StructureToPtr(camera, pointer, false);
            return pointer;
        }
    }
}
