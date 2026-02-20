// Copyright 2022-2026 Niantic Spatial.

using System.Collections.Generic;
using NianticSpatial.NSDK.AR.Utilities.Logging;
using NianticSpatial.NSDK.AR.XRSubsystems;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.XR.Management;

namespace NianticSpatial.NSDK.AR.Loader
{
    public class NsdkStandaloneLoader : XRLoaderHelper, INsdkInternalLoaderSupport
    {
        private NsdkLoaderHelper _nsdkLoaderHelper;
        private List<INsdkExternalLoader> _externalLoaders = new();

        /// <summary>
        /// The `XROcclusionSubsystem` whose lifecycle is managed by this loader.
        /// </summary>
        public XROcclusionSubsystem NsdkOcclusionSubsystem => base.GetLoadedSubsystem<XROcclusionSubsystem>();

        /// <summary>
        /// The `XRPersistentAnchorSubsystem` whose lifecycle is managed by this loader.
        /// </summary>
        public XRPersistentAnchorSubsystem NsdkPersistentAnchorSubsystem =>
            base.GetLoadedSubsystem<XRPersistentAnchorSubsystem>();

        /// <summary>
        /// The `XRMeshingSubsystem` whose lifecycle is managed by this loader.
        /// </summary>
        public XRMeshSubsystem NsdkMeshSubsystem => base.GetLoadedSubsystem<XRMeshSubsystem>();

        /// <summary>
        /// Initializes the loader. This is called from Unity when starting an AR session.
        /// </summary>
        /// <returns>`True` if the session subsystems were successfully created, otherwise `false`.</returns>
        public override bool Initialize()
        {
            _nsdkLoaderHelper = new NsdkLoaderHelper(_externalLoaders);
            return InitializeWithNsdkHelper(_nsdkLoaderHelper);
        }

        public bool InitializeWithNsdkHelper(NsdkLoaderHelper nsdkLoaderHelper)
        {
            _nsdkLoaderHelper = nsdkLoaderHelper;
            return _nsdkLoaderHelper.Initialize(this);
        }

        // There is no platform implementation for standalone.
        public bool IsPlatformDepthAvailable()
        {
            Log.Warning("Standalone currently has no platform implementation. You have to run with Playback enabled.");
            return false;
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

        /// <summary>
        /// Destroys each subsystem.
        /// </summary>
        /// <returns>Always returns `true`.</returns>
        public override bool Deinitialize()
        {
#if NIANTICSPATIAL_NSDK_AR_LOADER_ENABLED
            _nsdkLoaderHelper?.Deinitialize();
#endif
            return true;
        }

        public bool InitializePlatform()
        {
            Log.Warning("Standalone currently has no platform implementation. You have to run with Playback enabled.");
            return true;
        }

        public bool DeinitializePlatform()
        {
            Log.Warning("Standalone currently has no platform implementation. You have to run with Playback enabled.");
            return true;
        }

        void INsdkLoader.AddExternalLoader(INsdkExternalLoader loader)
        {
            _externalLoaders.Add(loader);
        }
    }
}
