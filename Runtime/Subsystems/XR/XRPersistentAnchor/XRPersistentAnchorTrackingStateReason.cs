// Copyright 2022-2026 Niantic Spatial.

using System;
using NianticSpatial.NSDK.AR.Utilities;

namespace NianticSpatial.NSDK.AR.XRSubsystems
{
    /// <summary>
    /// Provides further information about the tracking state of an anchor.
    /// Query this if the anchor's tracking state is NotTracking
    /// </summary>
    [PublicAPI]
    public enum TrackingStateReason : UInt32
    {
        /// <summary>
        /// No specific reason for the tracking state is available.
        /// </summary>
        None = 0,

        /// <summary>
        /// Tracking has been stopped for this anchor.
        /// </summary>
        Removed = 1,

        [Obsolete]
        AnchorTooFar = 2,

        /// <summary>
        /// This anchor is part of a private VPS map that this application
        /// does not have permission to localize to.
        /// </summary>
        PermissionDenied = 3,

        /// <summary>
        ///
        /// </summary>
        Initializing,
        InternalError,
        FatalNetworkError,
        NoVisualLocalization
    }
}
