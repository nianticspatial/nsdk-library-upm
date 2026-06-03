// Copyright 2022-2026 Niantic Spatial.

namespace NianticSpatial.NSDK.AR.Dashcam
{
    /// <summary>
    /// Configuration for the dashcam feature.
    /// Fields left at their default value of 0 will use native defaults.
    /// </summary>
    public struct DashcamConfiguration
    {
        /// <summary>
        /// Target FPS for buffering frames. The dashcam sub-samples the source frame stream
        /// to this rate. Default: 5 FPS.
        /// </summary>
        public int Framerate;

        /// <summary>
        /// Maximum seconds of data to keep in the circular buffer.
        /// Oldest frames are evicted when exceeded. Default: 60 seconds.
        /// </summary>
        public int MaxBufferSeconds;

        /// <summary>
        /// Maximum memory budget for the circular buffer in megabytes.
        /// Whichever limit (time or memory) is hit first triggers eviction. Default: 128 MB.
        /// </summary>
        public int MaxBufferMemoryMb;

        /// <summary>
        /// Base directory path for saving dashcam data.
        /// If empty, the platform's public application path is used.
        /// </summary>
        public string BasePath;
    }
}
