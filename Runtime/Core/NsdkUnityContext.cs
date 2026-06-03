// Copyright 2022-2026 Niantic Spatial.

using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using NianticSpatial.NSDK.AR.Utilities.Logging;
using NianticSpatial.NSDK.AR.Loader;
using UnityEngine;
using NianticSpatial.NSDK.AR.PAM;
using NianticSpatial.NSDK.AR.Utilities.Profiling;
using NianticSpatial.NSDK.AR.Settings;
using NianticSpatial.NSDK.AR.Telemetry;
using NianticSpatial.NSDK.Utilities.UnityAssets;

namespace NianticSpatial.NSDK.AR.Core
{
    /// <summary>
    /// [Experimental] <c>NsdkUnityContext</c> contains NSDK system components which are required by multiple modules.  This class should only be accessed by NSDK packages
    ///
    /// This Interface is experimental so may change or be removed from future versions without warning.
    /// </summary>
    public class NsdkUnityContext
    {
        /// <summary>
        /// <c>UnityContextHandle</c> holds a pointer to the native NSDK Unity context.  This is intended to be used only by NSDK packages.
        /// </summary>
        public static IntPtr UnityContextHandle { get; private set; } = IntPtr.Zero;

        internal static PlatformAdapterManager PlatformAdapterManager { get; private set; }
        private static EnvironmentConfig s_environmentConfig;
        private static UserConfig s_userConfig;
        internal static bool s_isDeviceLidarSupported = false;
        internal const string featureFlagFileName = "featureFlag.json";

        // Event triggered right before the context is destroyed. Used by internal code its lifecycle is not managed
        // by native UnityContext
        internal static event Action OnDeinitialized;
        internal static event Action OnUnityContextHandleInitialized;

        // Function that an external plugin can use to register its own PlatformDataAcquirer with PAM
        internal static Func<IntPtr, bool, bool, PlatformAdapterManager> CreatePamWithPlugin;

        internal static void Initialize(bool isDeviceLidarSupported, bool disableTelemetry = false, string featureFlagFilePath = "")
        {
#if NIANTICSPATIAL_NSDK_AR_LOADER_ENABLED
            s_isDeviceLidarSupported = isDeviceLidarSupported;

            if (UnityContextHandle != IntPtr.Zero)
            {
                Log.Warning($"Cannot initialize {nameof(NsdkUnityContext)} as it is already initialized");
                return;
            }

            var settings = NsdkSettingsHelper.ActiveSettings;

            Log.Info($"Initializing {nameof(NsdkUnityContext)}");
            s_environmentConfig = new EnvironmentConfig
            {
                ScanningEndpoint = settings.EndpointSettings.ScanningEndpoint,
                ScanningSqcEndpoint = settings.EndpointSettings.ScanningSqcEndpoint,
                SharedArEndpoint = settings.EndpointSettings.SharedArEndpoint,
                VpsEndpoint = settings.EndpointSettings.VpsEndpoint,
                VpsCoverageEndpoint = "",
                IdentityEndpoint = settings.EndpointSettings.IdentityEndpoint,
                PortalEndpoint = settings.EndpointSettings.PortalEndpoint,
                FastDepthEndpoint = settings.EndpointSettings.FastDepthSemanticsEndpoint,
                MediumDepthEndpoint = settings.EndpointSettings.DefaultDepthSemanticsEndpoint,
                SmoothDepthEndpoint = settings.EndpointSettings.SmoothDepthSemanticsEndpoint,
                FastSemanticsEndpoint = settings.EndpointSettings.FastDepthSemanticsEndpoint,
                MediumSemanticsEndpoint = settings.EndpointSettings.DefaultDepthSemanticsEndpoint,
                SmoothSemanticsEndpoint = settings.EndpointSettings.SmoothDepthSemanticsEndpoint,

                TelemetryEndpoint = "",
                BevEndpoint = settings.EndpointSettings.BevEndpoint,
                GeographiclibGeoidEndpoint = settings.EndpointSettings.GeographiclibGeoidEndpoint,
            };

            s_userConfig = new UserConfig
            {
                AccessToken = settings.AccessToken,
                FeatureFlagFilePath = string.IsNullOrEmpty(featureFlagFilePath) ? GetFeatureFlagPath() : featureFlagFilePath,
            };

            var deviceInfo = new DeviceInfo
            {
                AppId = Metadata.ApplicationId,
                Platform = Metadata.Platform,
                Manufacturer = Metadata.Manufacturer,
                ClientId = Metadata.ClientId,
                DeviceModel = Metadata.DeviceModel,
                AppInstanceId = Metadata.AppInstanceId,
                DeviceLidarSupported = isDeviceLidarSupported,
                // Unity returns WGS84 altitude (desired) on Android but MSL altitude (requires conversion) on iOS.
                // Until Unity releases a fix to align them, we need to convert MSL->WGS84 on iOS.
#if UNITY_IOS && !UNITY_EDITOR
                AltitudeIsMeanSeaLevel = true,
#else
                AltitudeIsMeanSeaLevel = false,
#endif
                DevEnvironment = Metadata.DevEnvironment,
            };

            UnityContextHandle = NativeApi.Lightship_ARDK_Unity_Context_Create(false, ref deviceInfo, ref s_environmentConfig, ref s_userConfig);

            Log.ConfigureLogger
            (
                UnityContextHandle,
                settings.UnityNsdkLogLevel,
                settings.FileNsdkLogLevel,
                settings.StdOutNsdkLogLevel
            );

#if NIANTICSPATIAL_NSDK_TELEMETRY_DISABLE
            const bool telemetryDisabledByScriptingDefine = true;
#else
            const bool telemetryDisabledByScriptingDefine = false;
#endif
            var telemetryDisabled = disableTelemetry || telemetryDisabledByScriptingDefine;

            if (!telemetryDisabled)
            {
                // Native process-wide sink drain (ardk::TelemetryPublisher); independent of C# TelemetryService.
                try
                {
                    if (!TelemetryPublishingNative.TryStart())
                    {
                        Log.Debug("Native telemetry publishing was already started.");
                    }
                }
                catch (Exception e)
                {
                    Log.Debug($"Failed to start native telemetry publishing: {e}");
                }
            }
            else
            {
                Log.Debug(
                    telemetryDisabledByScriptingDefine
                        ? "NIANTICSPATIAL_NSDK_TELEMETRY_DISABLE is set. Keeping telemetry disabled."
                        : "Detected a test run. Keeping telemetry disabled.");
                try
                {
                    TelemetrySinkNative.Disable();
                }
                catch (Exception e)
                {
                    Log.Warning($"Failed to disable native telemetry sink: {e}");
                }
            }
            OnUnityContextHandleInitialized?.Invoke();

            ProfilerUtility.RegisterProfiler(new UnityProfiler());
            ProfilerUtility.RegisterProfiler(new CTraceProfiler());

            CreatePam(settings);
#endif
        }

        private static void CreatePam(RuntimeNsdkSettings settings)
        {
            if (PlatformAdapterManager != null)
            {
                Log.Warning("Cannot create PAM as it is already created");
                return;
            }

            var isLidarEnabled = settings.PreferLidarIfAvailable && s_isDeviceLidarSupported;
            Log.Info($"Creating PAM (lidar enabled: {isLidarEnabled})");

            // Check if another NSDK plugin has registered with its own PlatformDataAcquirer.
            // Except if we're using playback, in which case we always use the SubsystemsDataAcquirer to read the dataset.
            if (null != CreatePamWithPlugin && !settings.UsePlayback)
            {
                PlatformAdapterManager =
                    CreatePamWithPlugin
                    (
                        UnityContextHandle,
                        isLidarEnabled,
                        settings.TestSettings.TickPamOnUpdate
                    );
            }
            else
            {
                PlatformAdapterManager =
                    PlatformAdapterManager.Create<PAM.NativeApi, SubsystemsDataAcquirer>
                    (
                        UnityContextHandle,
                        isLidarEnabled,
                        trySendOnUpdate: settings.TestSettings.TickPamOnUpdate
                    );
            }
        }

        private static void DisposePam()
        {
            Log.Info("Disposing PAM");

            PlatformAdapterManager?.Dispose();
            PlatformAdapterManager = null;
        }

        internal static void Deinitialize()
        {
            OnDeinitialized?.Invoke();
#if NIANTICSPATIAL_NSDK_AR_LOADER_ENABLED
            if (UnityContextHandle != IntPtr.Zero)
            {
                Log.Info($"Shutting down {nameof(NsdkUnityContext)}");

                DisposePam();

                try
                {
                    TelemetryPublishingNative.Stop();
                }
                catch (Exception e)
                {
                    Log.Debug($"Failed to stop native telemetry publishing: {e}");
                }

                if (!CheckUnityContext(UnityContextHandle))
                {
                    return;
                }

                NativeApi.Lightship_ARDK_Unity_Context_Shutdown(UnityContextHandle);
                UnityContextHandle = IntPtr.Zero;

                ProfilerUtility.ShutdownAll();
            }
#endif
        }

        internal static bool FeatureEnabled(string featureName)
        {
            if (!UnityContextHandle.IsValidHandle())
            {
                return false;
            }

            return NativeApi.Lightship_ARDK_Unity_Context_FeatureEnabled(UnityContextHandle, featureName);
        }

        private static string GetFeatureFlagPath()
        {
            var pathInPersistentData = Path.Combine(Application.persistentDataPath, featureFlagFileName);
            var pathInStreamingAsset = Path.Combine(Application.streamingAssetsPath, featureFlagFileName);
            var pathInTempCache = Path.Combine(Application.temporaryCachePath, featureFlagFileName);

            // Use if file exists in the persistent data path
            if (File.Exists(pathInPersistentData))
            {
                return pathInPersistentData;
            }

            // Use if file exists in the streaming asset path
            if (pathInStreamingAsset.Contains("://"))
            {
                // the file path is file URL e.g. on Android. copy to temp and use it
                bool fileRead = FileUtilities.TryReadAllText(pathInStreamingAsset, out var jsonString);
                if (fileRead)
                {
                    File.WriteAllText(pathInTempCache, jsonString);
                    return pathInTempCache;
                }
            }
            else
            {
                if (File.Exists(pathInStreamingAsset))
                {
                    return pathInStreamingAsset;
                }
            }

            // Write default setting to temp and use it
            const string defaultFeatureFlagSetting = @"{
                }";
            File.WriteAllText(pathInTempCache, defaultFeatureFlagSetting);
            return pathInTempCache;
        }

        public static IntPtr GetCoreContext(IntPtr unityContext)
        {
            if (!CheckUnityContext(unityContext))
            {
                return IntPtr.Zero;
            }
            return NativeApi.Lightship_ARDK_Unity_Context_GetCoreContext(unityContext);
        }

        public static IntPtr GetCommonContext(IntPtr unityContext)
        {
            if (!CheckUnityContext(unityContext))
            {
                return IntPtr.Zero;
            }
            return NativeApi.Lightship_ARDK_Unity_Context_GetCommonContext(unityContext);
        }

        public static IntPtr GetNSDKHandle(IntPtr unityContext)
        {
            if (!CheckUnityContext(unityContext))
            {
                return IntPtr.Zero;
            }

            return NativeApi.Lightship_ARDK_Unity_Context_GetARDKHandle(unityContext);
        }

        public static bool CheckUnityContext(IntPtr unityContext)
        {
            if (unityContext == IntPtr.Zero)
            {
                Log.Error("NSDK Unity Context is null.");
                return false;
            }

            return true;
        }

        /// <summary>
        /// Container to wrap the native NSDK C APIs
        /// </summary>
        private static class NativeApi
        {
            [DllImport(NsdkPlugin.Name)]
            public static extern IntPtr Lightship_ARDK_Unity_Context_Create(
                bool disableCtrace, ref DeviceInfo deviceInfo, ref EnvironmentConfig environmentConfig, ref UserConfig userConfig);

            [DllImport(NsdkPlugin.Name)]
            public static extern void Lightship_ARDK_Unity_Context_Shutdown(IntPtr unityContext);

            [DllImport(NsdkPlugin.Name)]
            public static extern bool Lightship_ARDK_Unity_Context_FeatureEnabled(IntPtr unityContext, string featureName);

            [DllImport(NsdkPlugin.Name)]
            public static extern IntPtr Lightship_ARDK_Unity_Context_GetCoreContext(IntPtr unityContext);

            [DllImport(NsdkPlugin.Name)]
            public static extern IntPtr Lightship_ARDK_Unity_Context_GetCommonContext(IntPtr unityContext);

            [DllImport(NsdkPlugin.Name)]
            public static extern IntPtr Lightship_ARDK_Unity_Context_GetARDKHandle(IntPtr unityContext);
        }


        // PLEASE NOTE: Do NOT add feature flags in this struct.
        [StructLayout(LayoutKind.Sequential)]
        private struct EnvironmentConfig
        {
            public string VpsEndpoint;
            public string VpsCoverageEndpoint;
            public string IdentityEndpoint;
            public string PortalEndpoint;
            public string SharedArEndpoint;
            public string FastDepthEndpoint;
            public string MediumDepthEndpoint;
            public string SmoothDepthEndpoint;
            public string FastSemanticsEndpoint;
            public string MediumSemanticsEndpoint;
            public string SmoothSemanticsEndpoint;
            public string ScanningEndpoint;
            public string ScanningSqcEndpoint;
            public string ObjectDetectionEndpoint;
            public string TelemetryEndpoint;
            public string BevEndpoint;
            public string GeographiclibGeoidEndpoint;
        }

        // PLEASE NOTE: Do NOT add feature flags in this struct.
        [StructLayout(LayoutKind.Sequential)]
        private struct UserConfig
        {
            public string AccessToken;
            public string FeatureFlagFilePath;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct DeviceInfo
        {
            public string AppId;
            public string Platform;
            public string Manufacturer;
            public string DeviceModel;
            public string ClientId;
            public string AppInstanceId;
            public bool DeviceLidarSupported;
            public bool AltitudeIsMeanSeaLevel;
            public string DevEnvironment;
        }
    }
}
