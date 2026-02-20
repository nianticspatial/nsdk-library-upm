// Copyright 2022-2026 Niantic Spatial.
using NianticSpatial.NSDK.AR.Utilities.Logging;
using NianticSpatial.NSDK.AR.Loader;
using NianticSpatial.NSDK.AR.Utilities;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.Scripting;
using UnityEngine.XR.ARSubsystems;

namespace NianticSpatial.NSDK.AR.Subsystems.Playback
{
    // Manages PlaybackDatasetReader
    [Preserve]
    public class NsdkPlaybackSessionSubsystem : XRSessionSubsystem, IPlaybackDatasetUser
    {
        /// <summary>
        /// Register the Lightship playback session subsystem.
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void Register()
        {
            Log.Info("NsdkPlaybackSessionSubsystem.Register");
            const string id = "Lightship-Playback-Session";

            var info = new XRSessionSubsystemDescriptor.Cinfo()
            {
                id = id,
                providerType = typeof(NsdkPlaybackProvider),
                subsystemTypeOverride = typeof(NsdkPlaybackSessionSubsystem),
                supportsInstall = false,
                supportsMatchFrameRate = true,
            };

#if UNITY_6000_0_OR_NEWER
            XRSessionSubsystemDescriptor.Register(info);
#else

            XRSessionSubsystemDescriptor.RegisterDescriptor(info);
#endif


        }

        void IPlaybackDatasetUser.SetPlaybackDatasetReader(PlaybackDatasetReader reader)
        {
            ((NsdkPlaybackProvider)provider).datasetReader = reader;
        }

        internal PlaybackDatasetReader GetPlaybackDatasetReader()
        {
            return ((NsdkPlaybackProvider)provider).datasetReader;
        }

        public bool TryMoveToNextFrame()
        {
            return ((NsdkPlaybackProvider)provider).TryMoveToNextFrame();
        }

        private class NsdkPlaybackProvider : Provider
        {
            public PlaybackDatasetReader datasetReader;

            private int m_initialApplicationFramerate;

            public override Promise<SessionAvailability> GetAvailabilityAsync()
            {
                var flag = SessionAvailability.Supported | SessionAvailability.Installed;
                return Promise<SessionAvailability>.CreateResolvedPromise(flag);
            }

            public override TrackingState trackingState
            {
                get => datasetReader?.GetCurrentTrackingState() ?? TrackingState.None;
            }

            public override bool matchFrameRateEnabled => matchFrameRateRequested;

            public override bool matchFrameRateRequested
            {
                get => true;
                set
                {
                    // TODO: investigate how this actually works, what should happen when value is set to false
                    if (value && datasetReader != null)
                    {
                        Application.targetFrameRate = datasetReader.GetFramerate();
                    }
                }
            }

            public override int frameRate => datasetReader?.GetFramerate() ?? 0;

            // start or resume
            public override void Start()
            {
                if (datasetReader == null)
                {
                    Log.Warning("Dataset reader is null, can't start NsdkPlaybackSessionSubsystem");
                    return;
                }

                datasetReader.Reset();
                if (!NsdkSettingsHelper.ActiveSettings.RunPlaybackManually)
                {
                    MonoBehaviourEventDispatcher.Updating.AddListener(MoveToNextFrame, 0);
                }
                else
                {
                    MonoBehaviourEventDispatcher.Updating.AddListener(MoveToNextFrameIfKeyDown, 0);
                }
            }

            // pause
            public override void Stop()
            {
                MonoBehaviourEventDispatcher.Updating.RemoveListener(MoveToNextFrame);
                MonoBehaviourEventDispatcher.Updating.RemoveListener(MoveToNextFrameIfKeyDown);
            }

            public override void Destroy()
            {
                datasetReader = null;
            }

            private void MoveToNextFrame()
            {
                datasetReader.TryMoveToNextFrame();
            }

            private void MoveToNextFrameIfKeyDown()
            {
#if UNITY_EDITOR
                // The Editor may be headless on CI
                if (null == Keyboard.current)
                    return;

                KeyControl space = InputSystem.GetDevice<Keyboard>().spaceKey;
                KeyControl forward = InputSystem.GetDevice<Keyboard>().rightArrowKey;
                KeyControl backward = InputSystem.GetDevice<Keyboard>().leftArrowKey;

                if (space.wasPressedThisFrame)
                    datasetReader.TryMoveToNextFrame();
                else if (space.isPressed)
                    return;

                if (forward.isPressed)
                    datasetReader.TryMoveToNextFrame();

                if (backward.isPressed)
                    datasetReader.TryMoveToPreviousFrame();
#else
                if (Input.touchCount == 2)
                    datasetReader.TryMoveToNextFrame();
#endif
            }

            public bool TryMoveToNextFrame()
            {
                if (running)
                    return datasetReader.TryMoveToNextFrame();

                return false;
            }
        }
    }
}
