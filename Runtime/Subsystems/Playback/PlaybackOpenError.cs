// Copyright 2026 Niantic Spatial.

using NianticSpatial.NSDK.AR.Utilities;

namespace NianticSpatial.NSDK.AR.Subsystems.Playback
{
    /// <summary>
    /// Why opening a scan's recording failed.
    /// </summary>
    [PublicAPI]
    public enum PlaybackOpenError
    {
        /// <summary>NSDK has no valid token, or the portal rejected it.</summary>
        NotAuthenticated = 1,
        /// <summary>The user may not read the scan.</summary>
        AccessDenied = 2,
        /// <summary>The portal has no scan with this ID.</summary>
        NotFound = 3,
        /// <summary>The scan has no validation recording that can play.</summary>
        NoPlaybackFile = 4,
        /// <summary>The scan's recording is not validated yet.</summary>
        NotReady = 5,
        /// <summary>The portal could not be reached, or answered with a server error.</summary>
        Network = 6,
        /// <summary>The download's size differs from the size the portal reported.</summary>
        SizeMismatch = 7,
        /// <summary>The recording could not be extracted, or has no capture.json.</summary>
        ExtractionFailed = 8,
        /// <summary>The extracted recording could not be opened as a dataset.</summary>
        OpenFailed = 9,
        /// <summary>The open was cancelled.</summary>
        Cancelled = 10,
        /// <summary>XR was deinitialized while the scan was opening.</summary>
        SessionEnded = 11
    }
}
