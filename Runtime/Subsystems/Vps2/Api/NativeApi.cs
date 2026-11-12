// Copyright Niantic Spatial.

using System;
using System.Runtime.InteropServices;
using NianticSpatial.NSDK.AR.API;
using NianticSpatial.NSDK.AR.Core;

namespace NianticSpatial.NSDK.AR.Subsystems.Vps2.Api
{
    internal class NativeApi : IApi
    {
        public IntPtr Construct(IntPtr unityContext) {
            return Native.Create(unityContext);
        }

        public void Destroy(IntPtr providerHandle)
        {
            Native.Destroy(providerHandle);
        }

        public NsdkStatus Start(IntPtr providerHandle)
        {
            return Native.Start(providerHandle);
        }

        public NsdkStatus Stop(IntPtr providerHandle)
        {
            return Native.Stop(providerHandle);
        }

        public NsdkFeatureStatus GetFeatureStatus(IntPtr providerHandle) {
            return Native.GetFeatureStatus(providerHandle);
        }

        public NsdkStatus Configure(IntPtr providerHandle, IApi.NsdkVps2Config config)
        {
            return Native.Configure(providerHandle, config);
        }

        public NsdkStatus GetLatestLocalization(IntPtr providerHandle, out IApi.NsdkVps2Localization localization)
        {
            return Native.GetLatestLocalization(providerHandle, out localization);
        }

        public NsdkStatus LocalizeToSite(IntPtr providerHandle, NsdkString siteId, NsdkString assetId)
        {
            return Native.LocalizeToSite(providerHandle, siteId, assetId);
        }

        public NsdkStatus LocalizeToNearbySites
        (
            IntPtr providerHandle,
            double latitude,
            double longitude,
            double radiusMeters
        )
        {
            return Native.LocalizeToNearbySites(providerHandle, latitude, longitude, radiusMeters);
        }

        public NsdkStatus StopLocalize(IntPtr providerHandle, NsdkString siteId)
        {
            return Native.StopLocalize(providerHandle, siteId);
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

        public NsdkStatus GetDeviceGeolocation(IntPtr providerHandle, HeadingMode headingMode, out IApi.NsdkVps2GeolocationData location)
        {
            return Native.GetDeviceGeolocation(providerHandle, (int)headingMode, out location);
        }

        public NsdkStatus GetPose(IApi.NsdkVps2Localization localization, NsdkGeolocationData location, out IApi.NsdkVps2Pose pose)
        {
            return Native.GetPose(ref localization, ref location, out pose);
        }

        public NsdkStatus GetPoseFromLatestLocalization(IntPtr providerHandle, NsdkGeolocationData location, out IApi.NsdkVps2Pose pose)
        {
            return Native.GetPoseFromLatestLocalization(providerHandle, ref location, out pose);
        }

        public NsdkStatus GetGeolocationFromLatestLocalization(IntPtr providerHandle, NsdkTransform pose, out IApi.NsdkVps2GeolocationData geolocation)
        {
            return Native.GetGeolocationFromLatestLocalization(providerHandle, ref pose, out geolocation);
        }

        public NsdkStatus GetLatestLocalizationRequestRecords(IntPtr providerHandle, out IntPtr networkRequestRecords, out int size, out IntPtr handle)
        {
            return Native.GetLatestLocalizationRequestRecords(providerHandle, out networkRequestRecords, out size, out handle);
        }

        public NsdkStatus GetLatestDebuggerLogs(IntPtr providerHandle, out IntPtr logs, out int count,
            out IntPtr handle)
        {
            return Native.GetLatestDebuggerLogs(providerHandle, out logs, out count, out handle);
        }

        public NsdkStatus GetSessionId(IntPtr providerHandle, ref byte[] sessionId)
        {
            return Native.GetSessionId(providerHandle, sessionId);
        }

        private static class Native
        {
            [DllImport(NsdkPlugin.Name, EntryPoint = "Lightship_ARDK_Unity_VPS2_Provider_Create")]
            public static extern IntPtr Create(IntPtr unityContext);

            [DllImport(NsdkPlugin.Name, EntryPoint = "Lightship_ARDK_Unity_VPS2_Provider_Destroy")]
            public static extern void Destroy(IntPtr providerHandle);

            [DllImport(NsdkPlugin.Name, EntryPoint = "Lightship_ARDK_Unity_VPS2_Provider_GetFeatureStatus")]
            public static extern NsdkFeatureStatus GetFeatureStatus(IntPtr providerHandle);

            [DllImport(NsdkPlugin.Name, EntryPoint = "Lightship_ARDK_Unity_VPS2_Provider_Configure")]
            public static extern NsdkStatus Configure(IntPtr providerHandle, IApi.NsdkVps2Config config);

            [DllImport(NsdkPlugin.Name, EntryPoint = "Lightship_ARDK_Unity_VPS2_Provider_Start")]
            public static extern NsdkStatus Start(IntPtr providerHandle);

            [DllImport(NsdkPlugin.Name, EntryPoint = "Lightship_ARDK_Unity_VPS2_Provider_Stop")]
            public static extern NsdkStatus Stop(IntPtr providerHandle);

            [DllImport(NsdkPlugin.Name, EntryPoint = "Lightship_ARDK_Unity_VPS2_Provider_GetLatestLocalization")]
            public static extern NsdkStatus GetLatestLocalization(IntPtr providerHandle, out IApi.NsdkVps2Localization localization);

            [DllImport(NsdkPlugin.Name, EntryPoint = "Lightship_ARDK_Unity_VPS2_Provider_GetDeviceGeolocation")]
            public static extern NsdkStatus GetDeviceGeolocation(IntPtr providerHandle, int headingMode, out IApi.NsdkVps2GeolocationData location);

            [DllImport(NsdkPlugin.Name, EntryPoint = "Lightship_ARDK_Unity_VPS2_Provider_GetPose")]
            public static extern NsdkStatus GetPose(ref IApi.NsdkVps2Localization localization, ref NsdkGeolocationData location, out IApi.NsdkVps2Pose pose);

            [DllImport(NsdkPlugin.Name, EntryPoint = "Lightship_ARDK_Unity_VPS2_Provider_GetPoseFromLatestLocalization")]
            public static extern NsdkStatus GetPoseFromLatestLocalization(IntPtr providerHandle, ref NsdkGeolocationData location, out IApi.NsdkVps2Pose pose);

            [DllImport(NsdkPlugin.Name, EntryPoint = "Lightship_ARDK_Unity_VPS2_Provider_GetGeolocationFromLatestLocalization")]
            public static extern NsdkStatus GetGeolocationFromLatestLocalization(IntPtr providerHandle, ref NsdkTransform pose, out IApi.NsdkVps2GeolocationData geolocation);

            [DllImport(NsdkPlugin.Name, EntryPoint = "Lightship_ARDK_Unity_VPS2_Provider_LocalizeToSite")]
            public static extern NsdkStatus LocalizeToSite(IntPtr providerHandle, NsdkString siteId, NsdkString assetId);

            [DllImport(NsdkPlugin.Name, EntryPoint = "Lightship_ARDK_Unity_VPS2_Provider_LocalizeToNearbySites")]
            public static extern NsdkStatus LocalizeToNearbySites
            (
                IntPtr providerHandle,
                double latitude,
                double longitude,
                double radiusMeters
            );

            [DllImport(NsdkPlugin.Name, EntryPoint = "Lightship_ARDK_Unity_VPS2_Provider_StopLocalize")]
            public static extern NsdkStatus StopLocalize(IntPtr providerHandle, NsdkString siteId);

            // assetId is marshalled as a nullable pointer: pass null for the currently
            // localized asset, or a single-element array to name one.
            [DllImport(NsdkPlugin.Name, EntryPoint = "Lightship_ARDK_Unity_VPS2_Provider_GetAssetTrackingData")]
            public static extern NsdkStatus GetAssetTrackingData
            (
                IntPtr providerHandle,
                NsdkString[] assetId,
                out IApi.NsdkVps2AssetTrackingData trackingData
            );

            [DllImport(NsdkPlugin.Name, EntryPoint = "Lightship_ARDK_Unity_VPS2_Provider_GetLatestLocalizationRequestRecords")]
            public static extern NsdkStatus GetLatestLocalizationRequestRecords(
                IntPtr providerHandle,
                out IntPtr networkRequestRecords,
                out int count,
                out IntPtr handle
            );

            [DllImport(NsdkPlugin.Name, EntryPoint = "Lightship_ARDK_Unity_VPS2_Provider_GetLatestDebuggerLogs")]
            public static extern NsdkStatus GetLatestDebuggerLogs(
                IntPtr providerHandle,
                out IntPtr logs,
                out int count,
                out IntPtr handle
            );

            [DllImport(NsdkPlugin.Name, EntryPoint = "Lightship_ARDK_Unity_VPS2_Provider_GetSessionId")]
            public static extern NsdkStatus GetSessionId(IntPtr providerHandle, [Out] byte[] sessionId);
        }
    }
}
