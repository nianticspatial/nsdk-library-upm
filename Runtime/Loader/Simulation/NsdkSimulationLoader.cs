// Copyright 2022-2026 Niantic Spatial.

using System.Collections.Generic;

using NianticSpatial.NSDK.AR.Core;
using NianticSpatial.NSDK.AR.Subsystems.Meshing;
using NianticSpatial.NSDK.AR.Subsystems.Playback;
using NianticSpatial.NSDK.AR.Utilities.Logging;
using NianticSpatial.NSDK.AR.XRSubsystems;

using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.XR.Management;

namespace NianticSpatial.NSDK.AR.Loader
{
    public class NsdkSimulationLoader : XRLoaderHelper, INsdkInternalLoaderSupport
    {
        private static List<XRSessionSubsystemDescriptor> s_SessionSubsystemDescriptors = new();
        private static List<XRInputSubsystemDescriptor> s_InputSubsystemDescriptors = new();
        private static List<XRPlaneSubsystemDescriptor> s_PlaneSubsystemDescriptors = new();
        private static List<XRPointCloudSubsystemDescriptor> s_PointCloudSubsystemDescriptors = new();
        private static List<XRImageTrackingSubsystemDescriptor> s_ImageTrackingSubsystemDescriptors = new();
        private static List<XRRaycastSubsystemDescriptor> s_RaycastSubsystemDescriptors = new();
        private static List<XRMeshSubsystemDescriptor> s_MeshSubsystemDescriptors = new();
        private static List<XRCameraSubsystemDescriptor> s_CameraSubsystemDescriptors = new();
        private static List<XROcclusionSubsystemDescriptor> _occlusionSubsystemDescriptors = new();
        private static List<XRPersistentAnchorSubsystemDescriptor> s_persistentAnchorSubsystemDescriptors = new();

        private NsdkLoaderHelper _nsdkLoaderHelper;
        private readonly List<INsdkExternalLoader> _externalLoaders = new List<INsdkExternalLoader>();
        private bool _useZBufferDepth = true;
        private bool _useSimulationPersistentAnchors = true;

        /// <summary>
        /// Initializes the loader. This is called from Unity when initializing XR.
        /// </summary>
        /// <returns>`True` if the session subsystems were successfully created, otherwise `false`.</returns>
        public override bool Initialize()
        {
            var settings = NsdkSettingsHelper.ActiveSettings;

            // we disable NSDK depth if we're use z-buffer depth
            settings.UseNsdkDepth =
                settings.UseNsdkDepth && !settings.NsdkSimulationParams.UseZBufferDepth;

            // we disable playback, can be removed once this is part of standalone loader
            settings.UsePlayback = false;

            _nsdkLoaderHelper = new NsdkLoaderHelper(_externalLoaders);

            return InitializeWithNsdkHelper(_nsdkLoaderHelper);
        }

        public bool InitializeWithNsdkHelper(NsdkLoaderHelper nsdkLoaderHelper)
        {
            _nsdkLoaderHelper = nsdkLoaderHelper;

            var settings = NsdkSettingsHelper.ActiveSettings;
            _useZBufferDepth = settings.NsdkSimulationParams.UseZBufferDepth;
            _useSimulationPersistentAnchors = settings.NsdkSimulationParams.UseSimulationPersistentAnchor;

            _nsdkLoaderHelper.Initialize(this);

            if (_useSimulationPersistentAnchors)
            {
                CreateSubsystem<XRPersistentAnchorSubsystemDescriptor, XRPersistentAnchorSubsystem>
                (
                    s_persistentAnchorSubsystemDescriptors,
                    "Nsdk-Simulation-PersistentAnchor"
                );
            }

            return true;
        }

        public void InjectNsdkLoaderHelper(NsdkLoaderHelper nsdkLoaderHelper)
        {
            _nsdkLoaderHelper = nsdkLoaderHelper;
        }

        /// <summary>
        /// Destroys each subsystem.
        /// </summary>
        /// <returns>Always returns `true`.</returns>
        public override bool Deinitialize()
        {
#if NIANTICSPATIAL_NSDK_AR_LOADER_ENABLED
            return _nsdkLoaderHelper.Deinitialize();
#else
            return true;
#endif
        }

        public bool InitializePlatform()
        {
            var input = new NsdkInputProvider();

            CreateSubsystem<XRSessionSubsystemDescriptor, XRSessionSubsystem>(s_SessionSubsystemDescriptors,
                "XRSimulation-Session");
            CreateSubsystem<XRInputSubsystemDescriptor, XRInputSubsystem>(s_InputSubsystemDescriptors,
                "LightshipInput");
            CreateSubsystem<XRPlaneSubsystemDescriptor, XRPlaneSubsystem>(s_PlaneSubsystemDescriptors,
                "XRSimulation-Plane");
            CreateSubsystem<XRPointCloudSubsystemDescriptor, XRPointCloudSubsystem>(s_PointCloudSubsystemDescriptors,
                "XRSimulation-PointCloud");
            CreateSubsystem<XRImageTrackingSubsystemDescriptor, XRImageTrackingSubsystem>(
                s_ImageTrackingSubsystemDescriptors, "XRSimulation-ImageTracking");
            CreateSubsystem<XRRaycastSubsystemDescriptor, XRRaycastSubsystem>(s_RaycastSubsystemDescriptors,
                "XRSimulation-Raycast");
            CreateSubsystem<XRCameraSubsystemDescriptor, XRCameraSubsystem>(s_CameraSubsystemDescriptors,
                "Nsdk-XRSimulation-Camera");
            if (_useZBufferDepth)
            {
                CreateSubsystem<XROcclusionSubsystemDescriptor, XROcclusionSubsystem>(_occlusionSubsystemDescriptors,
                    "Nsdk-Simulation-Occlusion");
            }

            if (GetLoadedSubsystem<XRSessionSubsystem>() == null)
            {
                Log.Error("Failed to load session subsystem.");
                return false;
            }

            return true;
        }

        public bool DeinitializePlatform()
        {
            DestroySubsystem<XRRaycastSubsystem>();
            DestroySubsystem<XRImageTrackingSubsystem>();
            DestroySubsystem<XRPointCloudSubsystem>();
            DestroySubsystem<XRPlaneSubsystem>();
            DestroySubsystem<XRInputSubsystem>();
            DestroySubsystem<XRCameraSubsystem>();
            DestroySubsystem<XRSessionSubsystem>();
            DestroySubsystem<XRPersistentAnchorSubsystem>();

            return true;
        }

        public bool IsPlatformDepthAvailable()
        {
            return _useZBufferDepth;
        }

        public new void CreateSubsystem<TDescriptor, TSubsystem>(List<TDescriptor> descriptors, string id) where TDescriptor : ISubsystemDescriptor where TSubsystem : ISubsystem
        {
            base.CreateSubsystem<TDescriptor, TSubsystem>(descriptors, id);
        }

        public new void DestroySubsystem<T>() where T : class, ISubsystem
        {
            base.DestroySubsystem<T>();
        }

        public new T GetLoadedSubsystem<T>() where T : class, ISubsystem
        {
            return base.GetLoadedSubsystem<T>();
        }

        void INsdkLoader.AddExternalLoader(INsdkExternalLoader loader)
        {
            _externalLoaders.Add(loader);
        }
    }
}
