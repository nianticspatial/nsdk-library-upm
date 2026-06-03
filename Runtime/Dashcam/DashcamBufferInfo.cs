// Copyright 2022-2026 Niantic Spatial.

namespace NianticSpatial.NSDK.AR.Dashcam
{
    /// <summary>
    /// Snapshot of the dashcam circular buffer state.
    /// </summary>
    public struct DashcamBufferInfo
    {
        /// <summary>
        /// Number of frames currently in the buffer.
        /// </summary>
        public uint FrameCount;

        /// <summary>
        /// Approximate memory used by the buffer in bytes.
        /// </summary>
        public ulong MemoryUsedBytes;

        /// <summary>
        /// Duration of buffered data in milliseconds.
        /// </summary>
        public ulong DurationMs;
    }
}
