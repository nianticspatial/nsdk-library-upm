// Copyright 2022-2026 Niantic Spatial.

using System;

namespace NianticSpatial.NSDK.AR.Loader
{
    public class OverloadPlaybackSettings : INsdkPlaybackSettings
    {
        private bool _usePlayback;

        public bool UsePlayback
        {
            get { return _usePlayback; }
            set { _usePlayback = value; }
        }

        private string _playbackDatasetPath;

        public string PlaybackDatasetPath
        {
            get { return _playbackDatasetPath; }
            set { _playbackDatasetPath = value; }
        }

        private bool _runPlaybackManually;

        public bool RunManually
        {
            get { return _runPlaybackManually; }
            set { _runPlaybackManually = value; }
        }

        private bool _loopInfinitely = false;

        public bool LoopInfinitely
        {
            get { return _loopInfinitely; }
            set { _loopInfinitely = value; }
        }

        private uint _numberOfIterations = 1;

        [Obsolete]
        public uint NumberOfIterations
        {
            get { return _numberOfIterations; }
            set { _numberOfIterations = value; }
        }

        private int _startFrame = 0;
        public int StartFrame
        {
            get { return _startFrame; }
            set { _startFrame = value; }
        }

        private int _endFrame = -1;
        public int EndFrame
        {
            get { return _endFrame; }
            set { _endFrame = value; }
        }

        internal OverloadPlaybackSettings()
        {
        }

        internal OverloadPlaybackSettings(INsdkPlaybackSettings source)
        {
            UsePlayback = source.UsePlayback;
            PlaybackDatasetPath = source.PlaybackDatasetPath;
            RunManually = source.RunManually;
            LoopInfinitely = source.LoopInfinitely;
            StartFrame = source.StartFrame;
            EndFrame = source.EndFrame;
        }
    }
}
