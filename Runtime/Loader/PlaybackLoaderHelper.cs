// Copyright 2022-2026 Niantic Spatial.

using System.Collections.Generic;
using NianticSpatial.NSDK.AR.Utilities.Logging;
using NianticSpatial.NSDK.AR.Loader;
using NianticSpatial.NSDK.AR.Subsystems.Playback;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.XR.Management;

namespace NianticSpatial.NSDK.AR
{
    internal class PlaybackLoaderHelper
    {
        private readonly List<XRSessionSubsystemDescriptor> _sessionSubsystemDescriptors = new();
        private readonly List<XRCameraSubsystemDescriptor> _cameraSubsystemDescriptors = new();
        private readonly List<XROcclusionSubsystemDescriptor> _occlusionSubsystemDescriptors = new();
        private readonly List<XRInputSubsystemDescriptor> _inputSubsystemDescriptors = new();

        private NsdkPlaybackInputProvider _inputProvider;

        internal PlaybackDatasetReader DatasetReader { get; private set; }

        /// <summary>
        /// Initializes the loader. This step has to be done before the native helper is initialized, so the native
        /// helper knows if lidar support is in the dataset.
        /// </summary>
        /// <returns>`True` if the session subsystems were successfully created, otherwise `false`.</returns>
        internal bool InitializeBeforeNativeHelper(INsdkInternalLoaderSupport loader)
        {
            Log.Info("Initialize Playback subsystems");

            var settings = NsdkSettingsHelper.ActiveSettings;

            var runtimeDataset = settings.PlaybackDataset;
            if (runtimeDataset != null)
            {
                Log.Debug($"Using settings: {settings} with a runtime playback dataset");
                if (runtimeDataset.IsClosed)
                {
                    Log.Error("Failed to initialize Playback subsystems because the runtime playback dataset is closed.");
                    return false;
                }
            }
            else
            {
                Log.Debug($"Using settings: {settings} with playback dataset path: {settings.PlaybackDatasetPath}");
            }

            // The app owns a runtime dataset; the loader owns a dataset it opens from the path.
            var dataset = runtimeDataset ?? PlaybackDataset.Open(settings.PlaybackDatasetPath);

            if (dataset == null)
            {
                Log.Error("Failed to initialize Playback subsystems because no dataset was loaded.");
                return false;
            }

            DatasetReader = CreateDatasetReader(
                dataset,
                runtimeDataset != null,
                settings.LoopPlaybackInfinitely,
                settings.PlaybackSettings.StartFrame,
                settings.PlaybackSettings.EndFrame);

            loader.CreateSubsystem<XRSessionSubsystemDescriptor, XRSessionSubsystem>
            (
                _sessionSubsystemDescriptors,
                "Lightship-Playback-Session"
            );

            var sessionSubsystem = loader.GetLoadedSubsystem<XRSessionSubsystem>();
            ((IPlaybackDatasetUser)sessionSubsystem).SetPlaybackDatasetReader(DatasetReader);

            loader.CreateSubsystem<XRCameraSubsystemDescriptor, XRCameraSubsystem>
            (
                _cameraSubsystemDescriptors,
                "Nsdk-Playback-Camera"
            );

            var cameraSubsystem = loader.GetLoadedSubsystem<XRCameraSubsystem>();

            if (sessionSubsystem == null)
            {
                // Subsystems can only be loaded in Play Mode
                Log.Error("Failed to load subsystem.");
                return false;
            }

            ((IPlaybackDatasetUser)cameraSubsystem).SetPlaybackDatasetReader(DatasetReader);

            if (dataset.LidarEnabled && (!settings.UseNsdkDepth || settings.PreferLidarIfAvailable))
            {
                loader.DestroySubsystem<XROcclusionSubsystem>();

                Log.Info("Creating " + nameof(NsdkPlaybackOcclusionSubsystem));
                loader.CreateSubsystem<XROcclusionSubsystemDescriptor, XROcclusionSubsystem>
                (
                    _occlusionSubsystemDescriptors,
                    "Lightship-Playback-Occlusion"
                );

                var occlusionSubsystem = loader.GetLoadedSubsystem<XROcclusionSubsystem>();
                ((IPlaybackDatasetUser)occlusionSubsystem).SetPlaybackDatasetReader(DatasetReader);
            }

            ConfigureLocationAndCompass(true, DatasetReader);

            return true;
        }

        /// <summary>
        /// Creates the reader over a dataset. A runtime dataset plays whole and stays open on unload; the trim
        /// frames apply only to a dataset opened from the path.
        /// </summary>
        internal static PlaybackDatasetReader CreateDatasetReader(
            PlaybackDataset dataset, bool isRuntimeDataset, bool loopInfinitely, int startFrame, int endFrame)
        {
            return isRuntimeDataset
                ? new PlaybackDatasetReader(dataset, loopInfinitely, ownsDataset: false)
                : new PlaybackDatasetReader(dataset, loopInfinitely, startFrame, endFrame);
        }

        /// <summary>
        /// Initializes the loader. This step has to be done after the native helper is initialized because the Unity
        /// context has to exist.
        /// </summary>
        /// <returns>`True` if the session subsystems were successfully created, otherwise `false`.</returns>
        internal bool InitializeAfterNativeHelper(INsdkInternalLoaderSupport loader)
        {
            // Input is an integrated subsystem that must be created after the NsdkUnityContext is initialized,
            // which is why it's done here instead of in the PlaybackLoaderHelper
            Log.Info("Creating " + nameof(NsdkPlaybackInputProvider));
            _inputProvider = new NsdkPlaybackInputProvider();
            _inputProvider.SetPlaybackDatasetReader(DatasetReader);

            loader.DestroySubsystem<XRInputSubsystem>();
            loader.CreateSubsystem<XRInputSubsystemDescriptor, XRInputSubsystem>
            (
                _inputSubsystemDescriptors,
                "LightshipInput"
            );

            return true;
        }

        /// <summary>
        /// Destroys each initialized subsystem.
        /// </summary>
        internal void Deinitialize(INsdkInternalLoaderSupport loader)
        {
            Log.Info("Deinitialize playback subsystems");
            if (loader == null)
            {
                Log.Warning("Loader is null. Assuming system is already deinitialized.");
                return;
            }

            DatasetReader?.UnloadDataset();
            _inputProvider.SetPlaybackDatasetReader(null);
            DatasetReader = null;
            ConfigureLocationAndCompass(false, null);

            var sessionSubsystem = loader.GetLoadedSubsystem<XRSessionSubsystem>();
            if (sessionSubsystem != null)
            {
                if (sessionSubsystem.running)
                {
                    sessionSubsystem.Stop();
                }

                loader.DestroySubsystem<XRSessionSubsystem>();
            }

            var xrCameraSubsystem = loader.GetLoadedSubsystem<XRCameraSubsystem>();
            if (xrCameraSubsystem != null)
            {
                if (xrCameraSubsystem.running)
                {
                    xrCameraSubsystem.Stop();
                }

                loader.DestroySubsystem<XRCameraSubsystem>();
            }
        }

        private static void ConfigureLocationAndCompass(bool isLoaded, PlaybackDatasetReader datasetReader)
        {
            ((IPlaybackDatasetUser)Input.location).SetPlaybackDatasetReader(datasetReader);
            ((IPlaybackDatasetUser)Input.compass).SetPlaybackDatasetReader(datasetReader);
        }
    }
}
