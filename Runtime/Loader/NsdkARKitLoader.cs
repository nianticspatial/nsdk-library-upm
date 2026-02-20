// Copyright 2022-2026 Niantic Spatial.

#if UNITY_IOS || UNITY_EDITOR

#if NIANTICSPATIAL_NSDK_AR_LOADER_ENABLED && UNITY_IOS && !UNITY_EDITOR
#define NIANTICSPATIAL_NSDK_ARKIT_LOADER_ENABLED
#endif

using System.Collections.Generic;
using NianticSpatial.NSDK.AR.Subsystems;
using NianticSpatial.NSDK.AR.Subsystems.Playback;
using NianticSpatial.NSDK.AR.Utilities.Logging;
using NianticSpatial.NSDK.AR.XRSubsystems;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.ARKit;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.XR.Management;

namespace NianticSpatial.NSDK.AR.Loader
{
    /// <summary>
    /// Manages the lifecycle of NSDK and ARKit subsystems.
    /// </summary>
    public class NsdkARKitLoader : ARKitLoader, INsdkInternalLoaderSupport
    {
        private NsdkLoaderHelper _nsdkLoaderHelper;
        private List<INsdkExternalLoader> _externalLoaders = new();

        /// <summary>
        /// The `XROcclusionSubsystem` whose lifecycle is managed by this loader.
        /// </summary>
        public XROcclusionSubsystem NsdkOcclusionSubsystem => ((XRLoaderHelper) this).GetLoadedSubsystem<XROcclusionSubsystem>();

        /// <summary>
        /// The `XRPersistentAnchorSubsystem` whose lifecycle is managed by this loader.
        /// </summary>
        public XRPersistentAnchorSubsystem NsdkPersistentAnchorSubsystem =>
            ((XRLoaderHelper) this).GetLoadedSubsystem<XRPersistentAnchorSubsystem>();

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

        public bool IsPlatformDepthAvailable()
        {
            var subsystems = new List<XRMeshSubsystem>();
            SubsystemManager.GetSubsystems(subsystems);
            return subsystems.Count > 0;
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

        /// <summary>
        /// This method does nothing. Subsystems must be started individually.
        /// </summary>
        /// <returns>Returns `true` on iOS. Returns `false` otherwise.</returns>
        public override bool Start()
        {
#if NIANTICSPATIAL_NSDK_ARKIT_LOADER_ENABLED
            return base.Start();
#else
            return false;
#endif
        }

        /// <summary>
        /// This method does nothing. Subsystems must be stopped individually.
        /// </summary>
        /// <returns>Returns `true` on iOS. Returns `false` otherwise.</returns>
        public override bool Stop()
        {
#if NIANTICSPATIAL_NSDK_ARKIT_LOADER_ENABLED
            return base.Stop();
#else
            return false;
#endif
        }

        /// <summary>
        /// Destroys each subsystem.
        /// </summary>
        /// <returns>Always returns `true`.</returns>
        public override bool Deinitialize()
        {
#if NIANTICSPATIAL_NSDK_ARKIT_LOADER_ENABLED
            return _nsdkLoaderHelper.Deinitialize();
#else
            return true;
#endif
        }

        public bool InitializePlatform() => base.Initialize();

        public bool DeinitializePlatform() => base.Deinitialize();
    }
}

#endif
