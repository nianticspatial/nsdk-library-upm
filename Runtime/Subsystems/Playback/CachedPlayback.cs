// Copyright 2026 Niantic Spatial.

using NianticSpatial.NSDK.AR.Utilities;

namespace NianticSpatial.NSDK.AR.Subsystems.Playback
{
    /// <summary>
    /// State of a scan in the playback cache.
    /// </summary>
    [PublicAPI]
    public enum CachedPlaybackState
    {
        /// <summary>The recording is fully downloaded and extracted.</summary>
        Ready = 0,
        /// <summary>This session is downloading or extracting the recording.</summary>
        Downloading = 1,
        /// <summary>The recording is partial and nothing is downloading it, as a killed app leaves it.</summary>
        Incomplete = 2
    }

    /// <summary>
    /// A scan in the playback cache.
    /// </summary>
    [PublicAPI]
    public readonly struct CachedPlayback
    {
        /// <summary>The scan ID.</summary>
        public string ScanId { get; }

        /// <summary>The state of the scan's cached recording.</summary>
        public CachedPlaybackState State { get; }

        internal CachedPlayback(string scanId, CachedPlaybackState state)
        {
            ScanId = scanId;
            State = state;
        }
    }
}
