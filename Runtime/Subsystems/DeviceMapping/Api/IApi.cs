// Copyright 2022-2026 Niantic Spatial.

using System;
using System.Runtime.InteropServices;

namespace NianticSpatial.NSDK.AR.Subsystems.DeviceMapping.Api
{
    internal interface IApi
    {
        /// <summary>
        /// Defined in ardk_mapping_configuration.h
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct NsdkDeviceMappingConfig
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

        IntPtr Construct(IntPtr unityContext);

        void Destroy(IntPtr providerHandle);

        void Start(IntPtr providerHandle);

        void Stop(IntPtr providerHandle);

        void Configure(IntPtr providerHandle, NsdkDeviceMappingConfig config);

        void StartMapping(IntPtr providerHandle);

        void StopMapping(IntPtr providerHandle);
    }
}
