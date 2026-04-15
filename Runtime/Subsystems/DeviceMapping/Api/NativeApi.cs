// Copyright 2022-2026 Niantic Spatial.

using System;
using System.Runtime.InteropServices;
using NianticSpatial.NSDK.AR.Core;
using NianticSpatial.NSDK.AR.Utilities;
namespace NianticSpatial.NSDK.AR.Subsystems.DeviceMapping.Api
{
    internal class NativeApi : IApi
    {
        public IntPtr Construct(IntPtr unityContext)
        {
            if (!NsdkUnityContext.CheckUnityContext(unityContext))
            {
                return IntPtr.Zero;
            }

            return Native.Create(unityContext);
        }

        public void Destroy(IntPtr providerHandle)
        {
            Native.Release(providerHandle);
        }

        public void Start(IntPtr providerHandle)
        {
            Native.Start(providerHandle);
        }

        public void Stop(IntPtr providerHandle)
        {
            Native.Stop(providerHandle);
        }

        public void Configure(IntPtr providerHandle, IApi.NsdkDeviceMappingConfig config)
        {
            Native.Configure(providerHandle, config);
        }

        public void StartMapping(IntPtr providerHandle)
        {
            Native.StartMapping(providerHandle);
        }

        public void StopMapping(IntPtr providerHandle)
        {
            Native.StopMapping(providerHandle);
        }

        private static class Native
        {
            [DllImport(NsdkPlugin.Name, EntryPoint = "Lightship_ARDK_Unity_Mapping_Create")]
            public static extern IntPtr Create(IntPtr unity_context);

            [DllImport(NsdkPlugin.Name, EntryPoint = "Lightship_ARDK_Unity_Mapping_Release")]
            public static extern void Release(IntPtr provider_handle);

            [DllImport(NsdkPlugin.Name, EntryPoint = "Lightship_ARDK_Unity_Mapping_Start")]
            public static extern void Start(IntPtr provider_handle);

            [DllImport(NsdkPlugin.Name, EntryPoint = "Lightship_ARDK_Unity_Mapping_Stop")]
            public static extern void Stop(IntPtr provider_handle);

            [DllImport(NsdkPlugin.Name, EntryPoint = "Lightship_ARDK_Unity_Mapping_Configure")]
            public static extern void Configure(IntPtr provider_handle, IApi.NsdkDeviceMappingConfig config);

            [DllImport(NsdkPlugin.Name, EntryPoint = "Lightship_ARDK_Unity_Mapping_StartMapping")]
            public static extern void StartMapping(IntPtr provider_handle);

            [DllImport(NsdkPlugin.Name, EntryPoint = "Lightship_ARDK_Unity_Mapping_StopMapping")]
            public static extern void StopMapping(IntPtr provider_handle);
        }
    }
}
