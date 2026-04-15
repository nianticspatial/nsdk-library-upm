// Copyright 2022-2026 Niantic Spatial.

using System.Collections.Generic;

using NianticSpatial.NSDK.AR.Utilities.Logging;
using NianticSpatial.NSDK.AR.Core;
using NianticSpatial.NSDK.AR.Subsystems.Meshing;

using NianticSpatial.NSDK.AR.Subsystems.Occlusion;
using NianticSpatial.NSDK.AR.Subsystems.Scanning;
using NianticSpatial.NSDK.AR.Subsystems.SceneSegmentation;
using NianticSpatial.NSDK.AR.Subsystems.DeviceMapping;
using NianticSpatial.NSDK.AR.Subsystems.Vps2;
using NianticSpatial.NSDK.AR.XRSubsystems;
using UnityEngine.XR;
using UnityEngine.XR.ARSubsystems;

namespace NianticSpatial.NSDK.AR.Loader
{
    public class NativeLoaderHelper
    {
        protected readonly List<XROcclusionSubsystemDescriptor> OcclusionSubsystemDescriptors = new();
        protected readonly List<XRSceneSegmentationSubsystemDescriptor> SceneSegmentationSubsystemDescriptors = new();
        protected readonly List<XRScanningSubsystemDescriptor> ScanningSubsystemDescriptors = new();
        protected readonly List<XRMeshSubsystemDescriptor> MeshingSubsystemDescriptors = new();

protected readonly List<XRVps2SubsystemDescriptor> Vps2SubsystemDescriptors = new ();
        protected readonly List<XRDeviceMappingSubsystemDescriptor> DeviceMappingSubsystemDescriptors = new();

        internal virtual bool Initialize(INsdkInternalLoaderSupport loader, bool isLidarSupported)
        {
            var settings = NsdkSettingsHelper.ActiveSettings;

            NsdkUnityContext.Initialize(isLidarSupported, settings.TestSettings.DisableTelemetry);

            Log.Info("Initialize native subsystems");

            // Create NSDK Occlusion subsystem
            if (settings.UseNsdkDepth && (!settings.PreferLidarIfAvailable || !isLidarSupported))
            {
                // Destroy the platform's depth subsystem before creating our own.
                // The native platform's loader will destroy our subsystem for us during Deinitialize.
                loader.DestroySubsystem<XROcclusionSubsystem>();

                Log.Info("Creating " + nameof(NsdkOcclusionSubsystem));
                loader.CreateSubsystem<XROcclusionSubsystemDescriptor, XROcclusionSubsystem>
                (
                    OcclusionSubsystemDescriptors,
                    "Nsdk-Occlusion"
                );
            }

            // Create VPS2 subsystem
            if (settings.UseNsdkVps2)
            {
                Log.Info("Creating " + nameof(NsdkVps2Subsystem));
                loader.CreateSubsystem<XRVps2SubsystemDescriptor, XRVps2Subsystem>
                (
                    Vps2SubsystemDescriptors,
                    "NSDK-VPS2"
                );
            }

            // Create Lightship Scene Segmentation subsystem
            if (settings.UseNsdkSceneSegmentation)
            {
                Log.Info("Creating " + nameof(NsdkSceneSegmentationSubsystem));
                loader.CreateSubsystem<XRSceneSegmentationSubsystemDescriptor, XRSceneSegmentationSubsystem>
                (
                    SceneSegmentationSubsystemDescriptors,
                    "Nsdk-SceneSegmentation"
                );
            }

            // Create Lightship Scanning subsystem
            if (settings.UseNsdkScanning)
            {
                Log.Info("Creating " + nameof(NsdkScanningSubsystem));
                loader.CreateSubsystem<XRScanningSubsystemDescriptor, XRScanningSubsystem>
                (
                    ScanningSubsystemDescriptors,
                    "Nsdk-Scanning"
                );
            }

            if (settings.UseNsdkMeshing)
            {
                // our C# "ghost" creates our meshing module to listen to Unity meshing lifecycle callbacks
                loader.DestroySubsystem<XRMeshSubsystem>();
                NsdkMeshingProvider.Construct(NsdkUnityContext.UnityContextHandle);

                // Create Unity integrated subsystem
                loader.CreateSubsystem<XRMeshSubsystemDescriptor, XRMeshSubsystem>
                (
                    MeshingSubsystemDescriptors,
                    "LightshipMeshing"
                );
            }

            if (settings.UseNsdkDeviceMapping)
            {
                Log.Info("Creating " + nameof(NsdkDeviceMappingSubsystem));
                loader.CreateSubsystem<XRDeviceMappingSubsystemDescriptor, XRDeviceMappingSubsystem>
                (
                    DeviceMappingSubsystemDescriptors,
                    "NSDK-DeviceMapping"
                );
            }

            return true;
        }

        /// <summary>
        /// Destroys each initialized subsystem.
        /// </summary>
        internal virtual void Deinitialize(INsdkInternalLoaderSupport loader)
        {
            Log.Info("Destroying lightship subsystems");
            if (loader == null)
            {
                Log.Warning("Loader is null. Assuming system is already deinitialized.");
                return;
            }

            // Destroy subsystem does a null check, so will just no-op if these subsystems were not created or already destroyed
            loader.DestroySubsystem<XRSceneSegmentationSubsystem>();
            loader.DestroySubsystem<XROcclusionSubsystem>();
            loader.DestroySubsystem<XRScanningSubsystem>();
            loader.DestroySubsystem<XRMeshSubsystem>();
            NsdkMeshingProvider.Reset();
            loader.DestroySubsystem<XRVps2Subsystem>();
            loader.DestroySubsystem<XRDeviceMappingSubsystem>();

            // Unity's native lifecycle handler for integrated subsystems does call Stop() before Shutdown() if
            // the subsystem is running when the latter is called. However, for the XRInputSubsystem, this causes
            // the below error to appear.
            //      "A device disconnection with the id 0 has been reported but no device with that id was connected."
            // Manually calling Stop() before Shutdown() eliminates the issue.
            var input = loader.GetLoadedSubsystem<XRInputSubsystem>();
            if (input != null && input.running)
            {
                input.Stop();
            }

            loader.DestroySubsystem<XRInputSubsystem>();

            NsdkUnityContext.Deinitialize();
        }
    }
}
