// Copyright 2022-2026 Niantic Spatial.

namespace NianticSpatial.NSDK.AR.Dashcam
{
    /// <summary>
    /// Status of a dashcam save operation.
    /// </summary>
    public enum DashcamSaveState
    {
        /// <summary>
        /// No save has been requested or the save result has not yet been computed.
        /// </summary>
        NotAvailable = 0,

        /// <summary>
        /// The save completed successfully.
        /// </summary>
        Saved = 1,

        /// <summary>
        /// The save failed (e.g. no frames in buffer, I/O error).
        /// </summary>
        Failed = 2,
    }
}
