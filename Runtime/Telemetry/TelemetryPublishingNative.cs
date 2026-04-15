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
        /// <param name="environment">Telemetry ingest environment (e.g. from <c>AuthEnvironmentType</c> via
        /// <c>ToString()</c>: Dev, Staging, Production).</param>
        /// <returns>False if publishing was already started.</returns>
        public static bool TryStart(string environment)
        {
            // Platform strings are parsed by ParseTelemetryPlatform in telemetry_publishing_api.cc.
            // Keep these values in sync with that function and TelemetryPlatformToIdValue in
            // telemetry_publisher.cc.
            string platform;

#if UNITY_EDITOR
            platform = "unity editor";
#elif UNITY_ANDROID
            platform = "unity android";
#elif UNITY_IOS
            platform = "unity ios";
#else
            // Non-Unity mobile targets (e.g. standalone): empty string maps to kUnknown,
            // so the platform field is omitted from telemetry id fields.
            platform = string.Empty;
#endif

            return Native.TelemetryPublishingStart(environment ?? string.Empty, platform);
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
                [MarshalAs(UnmanagedType.LPUTF8Str)] string environment,
                [MarshalAs(UnmanagedType.LPUTF8Str)] string platform);

            [DllImport(NsdkPlugin.Name, EntryPoint = "Lightship_ARDK_Unity_TelemetryPublishing_Stop")]
            public static extern void TelemetryPublishingStop();
        }
    }
}
