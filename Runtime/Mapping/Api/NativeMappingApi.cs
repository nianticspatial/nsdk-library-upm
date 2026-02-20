// Copyright 2022-2026 Niantic Spatial.

using System;
using System.Runtime.InteropServices;
using NianticSpatial.NSDK.AR.Core;
using NianticSpatial.NSDK.AR.Utilities;
using NianticSpatial.NSDK.AR.Utilities.Logging;
using Unity.Collections;
using UnityEngine;
using UnityEngine.XR.ARSubsystems;
using NianticSpatial.NSDK.AR.PersistentAnchors;

namespace NianticSpatial.NSDK.AR.Mapping
{
    internal class NativeMappingApi :
        IMappingApi
    {
        private IntPtr _nativeProviderHandle;

        public IntPtr Create(IntPtr unityContext)
        {
            if (!NsdkUnityContext.CheckUnityContext(unityContext))
            {
                return IntPtr.Zero;
            }
            _nativeProviderHandle = Lightship_ARDK_Unity_Mapping_Create(unityContext);
            return _nativeProviderHandle;
        }

        public void Dispose()
        {
            if (!CheckNativeHandle())
            {
                return;
            }
            Lightship_ARDK_Unity_Mapping_Release(_nativeProviderHandle);
            _nativeProviderHandle = IntPtr.Zero;
        }

        public void Start()
        {
            if (!CheckNativeHandle())
            {
                return;
            }
            Lightship_ARDK_Unity_Mapping_Start(_nativeProviderHandle);
        }

        public void Stop()
        {
            if (!CheckNativeHandle())
            {
                return;
            }
            Lightship_ARDK_Unity_Mapping_Stop(_nativeProviderHandle);
        }

        /// <summary>
        /// Defined in ardk_mapping_configuration.h file.
        /// Note: We need this because passing configurations as arguments breaks down
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        private struct MappingConfigurationCStruct
        {
            [MarshalAs(UnmanagedType.U1)]
            public bool trackingEdgesDisabled;

            [MarshalAs(UnmanagedType.U1)]
            public bool slickLearnedFeaturesEnabled;

            [MarshalAs(UnmanagedType.U1)]
            public bool forceCPULearnedFeatures;

            public UInt32 slickMapperFps;

            public float splitterMaxDistanceMeters;

            public float splitterMaxDurationSeconds;
        }

        public void Configure
        (
            bool trackingEdgesEnabled,
            bool slickLearnedFeaturesEnabled,
            bool useCpuLeanedFeatures,
            UInt32 slickMapperFps,
            float splitterMaxDistanceMeters,
            float splitterMaxDurationSeconds
        )
        {
            if (!CheckNativeHandle())
            {
                return;
            }

            var configurationCStruct = new MappingConfigurationCStruct();
            configurationCStruct.trackingEdgesDisabled = !trackingEdgesEnabled;
            configurationCStruct.slickLearnedFeaturesEnabled = slickLearnedFeaturesEnabled;
#if NIANTICSPATIAL_NSDK_ML2_ENABLED
            configurationCStruct.forceCPULearnedFeatures = true;
#else
            configurationCStruct.forceCPULearnedFeatures = useCpuLeanedFeatures;
#endif
            configurationCStruct.slickMapperFps = slickMapperFps;
            configurationCStruct.splitterMaxDistanceMeters = splitterMaxDistanceMeters;
            configurationCStruct.splitterMaxDurationSeconds = splitterMaxDurationSeconds;
            Lightship_ARDK_Unity_Mapping_Configure(_nativeProviderHandle, configurationCStruct);
        }

        public void StartMapping()
        {
            if (!CheckNativeHandle())
            {
                return;
            }
            Lightship_ARDK_Unity_Mapping_StartMapping(_nativeProviderHandle);
        }

        public void StopMapping()
        {
            if (!CheckNativeHandle())
            {
                return;
            }
            Lightship_ARDK_Unity_Mapping_StopMapping(_nativeProviderHandle);
        }

        private bool CheckNativeHandle()
        {
            if (!_nativeProviderHandle.IsValidHandle())
            {
                Debug.LogWarning("No valid Mapping module handle");
                return false;
            }
            return true;
        }

        // Native function declarations

        [DllImport(NsdkPlugin.Name)]
        private static extern IntPtr Lightship_ARDK_Unity_Mapping_Create(IntPtr unity_context);

        [DllImport(NsdkPlugin.Name)]
        private static extern void Lightship_ARDK_Unity_Mapping_Release(IntPtr feature_handle);

        [DllImport(NsdkPlugin.Name)]
        private static extern void Lightship_ARDK_Unity_Mapping_Start(IntPtr feature_handle);

        [DllImport(NsdkPlugin.Name)]
        private static extern void Lightship_ARDK_Unity_Mapping_Stop(IntPtr feature_handle);

        [DllImport(NsdkPlugin.Name)]
        private static extern void Lightship_ARDK_Unity_Mapping_ReleaseResource(IntPtr resourceHandle);

        [DllImport(NsdkPlugin.Name)]
        private static extern void Lightship_ARDK_Unity_Mapping_StartMapping(IntPtr feature_handle);


        [DllImport(NsdkPlugin.Name)]
        private static extern void Lightship_ARDK_Unity_Mapping_StopMapping(IntPtr feature_handle);

        [DllImport(NsdkPlugin.Name)]
        private static extern void Lightship_ARDK_Unity_Mapping_Configure
        (
            IntPtr feature_handle,
            MappingConfigurationCStruct config
        );
    }
}
