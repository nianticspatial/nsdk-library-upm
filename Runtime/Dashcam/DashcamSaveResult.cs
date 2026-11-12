// Copyright 2022-2026 Niantic Spatial.

namespace NianticSpatial.NSDK.AR.Dashcam
{
    /// <summary>
    /// Result of a dashcam save operation.
    /// </summary>
    public struct DashcamSaveResult
    {
        /// <summary>
        /// Current state of the save operation.
        /// </summary>
        public DashcamSaveState State;

        /// <summary>
        /// Path to the saved V2 sequence directory. Only valid when
        /// <see cref="State"/> is <see cref="DashcamSaveState.Saved"/>.
        /// </summary>
        public string SavePath;

        /// <summary>
        /// Path to the exported .tgz archive. Only valid when
        /// <see cref="State"/> is <see cref="DashcamSaveState.Saved"/>.
        /// </summary>
        public string ArchivePath;
    }
}
