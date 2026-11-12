// Copyright Niantic Spatial.

using System.Runtime.InteropServices;
using NianticSpatial.NSDK.AR.Core;

namespace NianticSpatial.NSDK.AR.Telemetry
{
    /// <summary>
    /// Native bindings for process-wide telemetry publishing: starts/stops <c>ardk::TelemetryPublisher</c>
    /// (periodic drain of <c>TelemetrySinkRegistry</c>). Started from
    /// <see cref="NianticSpatial.NSDK.AR.Core.NsdkUnityContext"/> when telemetry is enabled; stopped on context shutdown.
    /// </summary>
    public static class TelemetryPublishingNative
    {
        /// <returns>False if publishing was already started.</returns>
        public static bool TryStart()
        {
            // Dev platform strings are parsed by ParseTelemetryDevPlatform in telemetry_publishing_api.cc.
            // Keep these values in sync with that function and TelemetryDevPlatformToIdValue in
            // telemetry_publisher.cc.
            string devPlatform;

#if UNITY_EDITOR
            devPlatform = "unity editor";
#elif UNITY_ANDROID
            devPlatform = "unity android";
#elif UNITY_IOS
            devPlatform = "unity ios";
#else
            // Non-Unity mobile targets (e.g. standalone): empty string maps to kUnknown,
            // so the dev_platform id field is omitted from telemetry id fields.
            devPlatform = string.Empty;
#endif

            return Native.TelemetryPublishingStart(devPlatform);
        }

        public static void Stop()
        {
            Native.TelemetryPublishingStop();
        }

        private static class Native
        {
            [DllImport(NsdkPlugin.Name, EntryPoint = "Lightship_ARDK_Unity_TelemetryPublishing_Start")]
            [return: MarshalAs(UnmanagedType.I1)]
            public static extern bool TelemetryPublishingStart(
                [MarshalAs(UnmanagedType.LPUTF8Str)] string devPlatform);

            [DllImport(NsdkPlugin.Name, EntryPoint = "Lightship_ARDK_Unity_TelemetryPublishing_Stop")]
            public static extern void TelemetryPublishingStop();
        }
    }
}
