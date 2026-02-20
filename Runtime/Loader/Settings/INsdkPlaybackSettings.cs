// Copyright 2022-2026 Niantic Spatial.

namespace NianticSpatial.NSDK.AR.Loader
{
    public interface INsdkPlaybackSettings
    {
        bool UsePlayback { get; set; }

        string PlaybackDatasetPath { get; set; }

        bool RunManually { get; set; }

        bool LoopInfinitely { get; set; }

        int StartFrame { get; set; }

        int EndFrame { get; set; }
    }
}
