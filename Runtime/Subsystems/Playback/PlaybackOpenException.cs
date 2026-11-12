// Copyright 2026 Niantic Spatial.

using System;
using NianticSpatial.NSDK.AR.Utilities;

namespace NianticSpatial.NSDK.AR.Subsystems.Playback
{
    /// <summary>
    /// Opening a scan's recording failed.
    /// </summary>
    [PublicAPI]
    public class PlaybackOpenException : Exception
    {
        /// <summary>Why the open failed.</summary>
        public PlaybackOpenError Error { get; }

        /// <summary>Creates an exception for <paramref name="error"/>.</summary>
        public PlaybackOpenException(PlaybackOpenError error)
            : base($"Opening the scan's recording failed: {error}")
        {
            Error = error;
        }
    }
}
