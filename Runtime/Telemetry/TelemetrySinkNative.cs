// Copyright Niantic Spatial.

using System.Runtime.InteropServices;
using NianticSpatial.NSDK.AR.Core;

namespace NianticSpatial.NSDK.AR.Telemetry
{
    /// <summary>
    /// Native bindings for publishing telemetry events into the process-wide
    /// <c>ardk::TelemetrySinkRegistry</c> (same sink drained by <c>TelemetryPublishingNative</c>).
    /// </summary>
    internal static class TelemetrySinkNative
    {
        public static void PublishInitializationEvent(string installMode, string processor)
        {
            Native.TelemetrySinkPublishInitializationEvent(
                installMode ?? string.Empty,
                processor ?? string.Empty);
        }

        public static void PublishArSessionStartEvent()
        {
            Native.TelemetrySinkPublishArSessionStartEvent();
        }

        public static void Disable()
        {
            Native.TelemetrySinkDisable();
        }

        private static class Native
        {
            [DllImport(NsdkPlugin.Name,
                EntryPoint = "Lightship_ARDK_Unity_TelemetrySink_PublishInitializationEvent")]
            public static extern void TelemetrySinkPublishInitializationEvent(
                [MarshalAs(UnmanagedType.LPUTF8Str)] string installMode,
                [MarshalAs(UnmanagedType.LPUTF8Str)] string processor);

            [DllImport(NsdkPlugin.Name,
                EntryPoint = "Lightship_ARDK_Unity_TelemetrySink_PublishArSessionStartEvent")]
            public static extern void TelemetrySinkPublishArSessionStartEvent();

            [DllImport(NsdkPlugin.Name, EntryPoint = "Lightship_ARDK_Unity_TelemetrySink_Disable")]
            public static extern void TelemetrySinkDisable();
        }
    }
}
