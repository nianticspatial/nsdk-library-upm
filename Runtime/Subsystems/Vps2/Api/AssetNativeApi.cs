// Copyright 2026 Niantic Spatial.

using System;
using System.Runtime.InteropServices;
using NianticSpatial.NSDK.AR.API;
using NianticSpatial.NSDK.AR.Core;

namespace NianticSpatial.NSDK.AR.Subsystems.Vps2.Api
{
    internal class AssetNativeApi : IAssetApi
    {
        public IntPtr Construct(IntPtr unityContext)
        {
            return Native.Construct(unityContext);
        }

        public void Destroy(IntPtr providerHandle)
        {
            Native.Destroy(providerHandle);
        }

        public NsdkStatus GetLatestLocalization(IntPtr providerHandle, out IApi.NsdkVps2Localization localization)
        {
            return Native.GetLatestLocalization(providerHandle, out localization);
        }

        public NsdkStatus GetAssetTrackingData
        (
            IntPtr providerHandle,
            NsdkString[] assetId,
            out IApi.NsdkVps2AssetTrackingData trackingData
        )
        {
            return Native.GetAssetTrackingData(providerHandle, assetId, out trackingData);
        }

        private static class Native
        {
            [DllImport(NsdkPlugin.Name, EntryPoint = "Lightship_ARDK_Unity_VPS2_AssetProvider_Create")]
            public static extern IntPtr Construct(IntPtr unityContext);

            [DllImport(NsdkPlugin.Name, EntryPoint = "Lightship_ARDK_Unity_VPS2_AssetProvider_Destroy")]
            public static extern void Destroy(IntPtr providerHandle);

            [DllImport(NsdkPlugin.Name, EntryPoint = "Lightship_ARDK_Unity_VPS2_AssetProvider_GetLatestLocalization")]
            public static extern NsdkStatus GetLatestLocalization
            (
                IntPtr providerHandle,
                out IApi.NsdkVps2Localization localization
            );

            [DllImport(NsdkPlugin.Name, EntryPoint = "Lightship_ARDK_Unity_VPS2_AssetProvider_GetAssetTrackingData")]
            public static extern NsdkStatus GetAssetTrackingData
            (
                IntPtr providerHandle,
                NsdkString[] assetId,
                out IApi.NsdkVps2AssetTrackingData trackingData
            );
        }
    }
}
