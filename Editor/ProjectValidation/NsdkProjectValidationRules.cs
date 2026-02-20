// Copyright 2022-2025 Niantic.
using System;
using System.Collections.Generic;
using System.IO;
using NianticSpatial.NSDK.AR.Loader;
using Unity.XR.CoreUtils.Editor;
using UnityEditor;
using UnityEditor.XR.Management;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.XR.ARKit;
using JetBrains.Annotations;
using System.Text.RegularExpressions;
using UnityEditor.XR.Management.Metadata;
using System.Reflection;
using System.Linq;
using UnityEngine.XR.ARFoundation;

#if MODULE_URP_ENABLED
using UnityEngine.Rendering.Universal;
#endif

namespace NianticSpatial.NSDK.AR.Editor
{
    internal static class NsdkProjectValidationRules
    {
        private static readonly NsdkSettings s_settings = NsdkSettings.Instance;
        private static readonly Version s_minGradleVersion = new(6, 7, 1);
        private static readonly Version s_minUnityVersion = new(2021, 1);

        private const string XRPlugInManagementPath = "Project/XR Plug-in Management";
        private const string NSDKPath = XRPlugInManagementPath + "/Niantic Spatial Development Kit";
        private const string PreferencesExternalToolsPath = "Preferences/External Tools";
        private const string Category = "Niantic Spatial Development Kit";
        private const string PlaybackDatasetMetaFilename = "capture.json";
        private const string UnityDownloadsPage = "https://unity.com/download";
        private const string CreateAPIKeyHelpLink = "https://nianticspatial.com/docs/beta/ardk/install/#adding-your-api-key-to-your-unity-project";
        private const string UpdateGradleVersionHelpLink = "https://nianticspatial.com/docs/beta/ardk/install/#installing-gradle-for-android";

        private static Dictionary<BuildTargetGroup, List<BuildValidationRule>> s_platformRules;

        internal static Dictionary<BuildTargetGroup, List<BuildValidationRule>> PlatformRules
        {
            get
            {
                if (s_platformRules == null || s_platformRules.Count == 0)
                {
                    s_platformRules = CreateNSDKValidationRules();
                }

                return s_platformRules;
            }
        }

        [InitializeOnLoadMethod]
        private static void AddNSDKValidationRules()
        {
            BuildValidator.AddRules(BuildTargetGroup.Standalone, PlatformRules[BuildTargetGroup.Standalone]);
            BuildValidator.AddRules(BuildTargetGroup.Android, PlatformRules[BuildTargetGroup.Android]);
            BuildValidator.AddRules(BuildTargetGroup.iOS, PlatformRules[BuildTargetGroup.iOS]);
        }

        private static Dictionary<BuildTargetGroup, List<BuildValidationRule>> CreateNSDKValidationRules()
        {
            var platformRules = new Dictionary<BuildTargetGroup, List<BuildValidationRule>>
            {
                { BuildTargetGroup.Standalone, new List<BuildValidationRule>() },
                { BuildTargetGroup.Android, new List<BuildValidationRule>() },
                { BuildTargetGroup.iOS, new List<BuildValidationRule>() }
            };

            var standaloneGlobalRules = CreateGlobalRules(
                s_settings,
                "StreamingAssets",
                GetStandaloneIsNsdkPluginEnabled,
                GetUnityVersion);
            var standaloneRules = CreateStandaloneRules(
                GetStandaloneIsNsdkPluginEnabled,
                GetSimulationPluginsStatusMatches);

            var androidGlobalRules = CreateGlobalRules(
                s_settings,
                "StreamingAssets",
                GetAndroidIsNsdkPluginEnabled,
                GetUnityVersion);
            var androidRules = CreateAndroidRules(
                GetAndroidIsNsdkPluginEnabled,
                GetAndroidTargetSdkVersion,
                GetAndroidGradleVersion,
                GetActiveBuildTarget);

            var iosGlobalRules = CreateGlobalRules(
                s_settings,
                "StreamingAssets",
                GetIosIsNsdkPluginEnabled,
                GetUnityVersion);
            var iosRules = CreateIOSRules(
                s_settings,
                GetIosIsNsdkPluginEnabled,
                GetIosTargetOsVersionString,
                GetIosLocationUsageDescription);

            platformRules[BuildTargetGroup.Standalone].AddRange(standaloneRules);
            platformRules[BuildTargetGroup.Standalone].Add(standaloneGlobalRules[0]);
            platformRules[BuildTargetGroup.Standalone].Add(standaloneGlobalRules[1]);
            platformRules[BuildTargetGroup.Standalone].Add(standaloneGlobalRules[3]);

            platformRules[BuildTargetGroup.Android].AddRange(androidRules);
            platformRules[BuildTargetGroup.Android].Add(androidGlobalRules[0]);
            platformRules[BuildTargetGroup.Android].Add(androidGlobalRules[1]);
            platformRules[BuildTargetGroup.Android].Add(androidGlobalRules[2]);

            platformRules[BuildTargetGroup.iOS].AddRange(iosRules);
            platformRules[BuildTargetGroup.iOS].Add(iosGlobalRules[0]);
            platformRules[BuildTargetGroup.iOS].Add(iosGlobalRules[1]);
            platformRules[BuildTargetGroup.iOS].Add(iosGlobalRules[2]);

            return platformRules;
        }

        internal static BuildValidationRule[] CreateGlobalRules
        (
            NsdkSettings nsdkSettings,
            string datasetContainingDirectory,
            [NotNull] Func<bool> getIsLightshipPluginEnabled,
            [NotNull] Func<string> getUnityVersion
        )
        {

            // URP validation. Only check if we're using URP
#if MODULE_URP_ENABLED
            var nonBackgroundRendererDataList = new Dictionary<string, ScriptableRendererData>();

            var globalRulesList = new List<BuildValidationRule>
            {
                new BuildValidationRule
                {
                    Category = Category,
                    Message = "Ensure Universal Render Pipeline is present and Render Pipeline Assets for AR have the 'AR Background Renderer Feature'.",
                    CheckPredicate = () =>
                    {
                        nonBackgroundRendererDataList.Clear();
                        // Check if URP package is present
                        var urpType = Type.GetType("UnityEngine.Rendering.Universal.UniversalRenderPipelineAsset, Unity.RenderPipelines.Universal.Runtime");
                        // filter out possible assets on Packages folder
                        var guids = AssetDatabase.FindAssets("t:UniversalRenderPipelineAsset");
                        var filteredGuids = guids
                        .Select(guid => AssetDatabase.GUIDToAssetPath(guid))
                        .Where(path => !path.StartsWith("Packages"))
                        .Select(path => AssetDatabase.AssetPathToGUID(path))
                        .ToArray();
                        if (urpType == null || filteredGuids.Length == 0)
                        {
                            return false;
                        }

                        FindNonBackgroundRendererData(nonBackgroundRendererDataList, filteredGuids);

                        // if all the URP assets have the ARBackgroundRendererFeature check passes.
                        return nonBackgroundRendererDataList.Count == 0;
                    },
                    IsRuleEnabled = () => true,
                    FixIt = () =>
                    {
                        var urpType = Type.GetType("UnityEngine.Rendering.Universal.UniversalRenderPipelineAsset, Unity.RenderPipelines.Universal.Runtime");
                        if (urpType == null)
                        {
                            return;
                        }
                        // If there are no urp assets create and set one as default
                        if(nonBackgroundRendererDataList.Count <= 0)
                        {
                            var rendererData = CreateURPAssets();
                            var guid = AssetDatabase.AssetPathToGUID("Assets/DefaultUniversalRenderer.asset");
                            nonBackgroundRendererDataList[guid] = rendererData;
                        }
                        // Add the ARBackgroundRendererFeature to all URP assets that are missing it.
                        nonBackgroundRendererDataList.Values.ToList().ForEach(ConfigureRendererFeatures);
                        AssetDatabase.SaveAssets();
                    },
                    FixItMessage = "Ensure URP assets for AR have the 'AR Background Renderer Feature'. If none exist, a default asset will be created. No action is required if " +
                        "URP assets for non-AR rendering are missing the 'AR Background Renderer Feature'.",
                    FixItAutomatic = false,
                    Error = false
                }
            };
#else
            var globalRulesList = new List<BuildValidationRule>();
#endif

            var globalRules = new[]
            {
                new BuildValidationRule
                {
                    Category = Category,
                    Message = "If using NSDK, it is recommended to use Unity version 2021.1 LTS or higher.",
                    CheckPredicate = () => new Version(getUnityVersion.Invoke()) >= s_minUnityVersion,
                    IsRuleEnabled = getIsLightshipPluginEnabled.Invoke,
                    FixItMessage = "Open the Unity project in Unity version 2021.1 LTS or higher.",
                    FixItAutomatic = false,
                    HelpLink = UnityDownloadsPage
                },
                // TODO: Remove this rule, and update tests to point correct test rules...
                new BuildValidationRule
                {
                    Category = Category,
#if NIANTICSPATIAL_NSDK_APIKEY_ENABLED
                    Message = "If using NSDK, please either login, disable developer authentication, or set the Lightship API Key provided in your project at https://lightship.dev/account/projects",
#else
                    Message = "If using NSDK, please either login or disable developer authentication",
#endif
                    // Use the same predicate for now whether NIANTICSPATIAL_NSDK_APIKEY_ENABLED is set or not.
                    // This is not ideal, but fixes the build jobs that inject the API key into the build pipeline.
                    CheckPredicate = () => !string.IsNullOrWhiteSpace(nsdkSettings.ApiKey) || !string.IsNullOrEmpty(nsdkSettings.RefreshToken) || !nsdkSettings.UseDeveloperAuthentication,
                    IsRuleEnabled = getIsLightshipPluginEnabled.Invoke,
                    FixIt = () => SettingsService.OpenProjectSettings(NSDKPath),
                    FixItMessage = "Open `Project Settings` > `XR Plug-in Management` > `Niantic Spatial Development Kit` and set the Lightship API Key provided by the Lightship Portal.",
                    HelpText = "For further assistance, follow the instructions in the Lightship SDK docs.",
                    HelpLink = CreateAPIKeyHelpLink,
                    FixItAutomatic = false,
                    Error = false
                },
                new BuildValidationRule
                {
                    Category = Category,
                    Message = "If using NSDK Device Playback feature, set the device playback dataset path to a valid dataset directory path in the StreamingAssets directory.",
                    CheckPredicate = () =>
                    {
                        var datasetPath = nsdkSettings.DevicePlaybackSettings.PlaybackDatasetPath;
                        if (string.IsNullOrEmpty(datasetPath) || string.IsNullOrEmpty(datasetContainingDirectory))
                        {
                            return false;
                        }
                        // normalize paths using GetFullPath() so that string comparison works on osx or windows
                        var fullPathDatasetContainingDir = Path.GetFullPath(datasetContainingDirectory, Application.dataPath);
                        var fullPathDatasetPath = Path.GetFullPath(datasetPath);

                        var isInStreamingAssets = fullPathDatasetPath.StartsWith(fullPathDatasetContainingDir);
                        var doesFolderExist = Directory.Exists(datasetPath);
                        var doesMetafileExist = File.Exists(Path.Combine(datasetPath, PlaybackDatasetMetaFilename));
                        return isInStreamingAssets && doesFolderExist && doesMetafileExist;
                    },
                    IsRuleEnabled = () =>
                        getIsLightshipPluginEnabled.Invoke() &&
                        nsdkSettings.DevicePlaybackSettings.UsePlayback,
                    FixIt = () =>
                    {
                        SettingsService.OpenProjectSettings(NSDKPath);
                    },
                    FixItMessage = "Open `Project Settings` > `XR Plug-in Management` > `Niantic Spatial Development Kit` > `Device` and set the device playback dataset path to a valid dataset directory path in the StreamingAssets directory.",
                    FixItAutomatic = false,
                    Error = false
                },
                new BuildValidationRule
                {
                    Category = Category,
                    Message = "If using NSDK Editor Playback feature, set the editor playback dataset path to a valid dataset directory path.",
                    CheckPredicate = () =>
                    {
                        var datasetPath = nsdkSettings.EditorPlaybackSettings.PlaybackDatasetPath;
                        if (string.IsNullOrEmpty(datasetPath))
                        {
                            return false;
                        }
                        var doesFolderExist = Directory.Exists(datasetPath);
                        var doesMetafileExist = File.Exists(Path.Combine(datasetPath, PlaybackDatasetMetaFilename));
                        return doesFolderExist && doesMetafileExist;
                    },
                    IsRuleEnabled = () =>
                        getIsLightshipPluginEnabled.Invoke() &&
                        nsdkSettings.EditorPlaybackSettings.UsePlayback,
                    FixIt = () =>
                    {
                        SettingsService.OpenProjectSettings(NSDKPath);
                    },
                    FixItMessage = "Open `Project Settings` > `XR Plug-in Management` > `Niantic Spatial Development Kit` > `Editor` and set the editor playback dataset path to a valid dataset directory path.",
                    FixItAutomatic = false,
                    Error = true
                }
            };
            globalRulesList.AddRange(globalRules);
            return globalRulesList.ToArray();
        }

        internal static BuildValidationRule[] CreateIOSRules(
            NsdkSettings nsdkSettings,
            [NotNull] Func<bool> getIosIsLightshipPluginEnabled,
            [NotNull] Func<string> getIosTargetOsVersionString,
            [NotNull] Func<string> getIosLocationUsageDescription)
        {
            var iOSRules = new[]
            {
                new BuildValidationRule
                {
                    Category = Category,
                    Message = "If using NSDK for iOS, enable the 'Niantic Spatial Development Kit' plug-in.",
                    CheckPredicate = getIosIsLightshipPluginEnabled.Invoke,
                    FixItMessage = "Open `Project Settings` > `XR Plug-in Management` > `iOS settings` and enable the 'Niantic Spatial Development Kit' plug-in.",
                    FixIt = () =>
                    {
                        var generalSettings = XRGeneralSettingsPerBuildTarget.XRGeneralSettingsForBuildTarget(BuildTargetGroup.iOS);
                        if (null == generalSettings)
                            return;

                        var managerSettings = generalSettings.AssignedSettings;
                        if (null == managerSettings)
                            return;

                        XRPackageMetadataStore.AssignLoader(managerSettings, typeof(NsdkARKitLoader).FullName, BuildTargetGroup.iOS);
                    },
                    FixItAutomatic = true,
                    Error = false
                },
                new BuildValidationRule
                {
                    Category = Category,
                    Message = $"If using NSDK Depth, Meshing, or Semantics features for iOS, set the target iOS version to 13.0 or higher (currently {getIosTargetOsVersionString.Invoke()}).",
                    CheckPredicate = () => OSVersion.Parse(getIosTargetOsVersionString.Invoke()) >= new OSVersion(13),
                    IsRuleEnabled = () =>
                    {
                        var isFeatureEnabled =
                            nsdkSettings.UseNsdkDepth ||
                            nsdkSettings.UseNsdkMeshing ||
                            nsdkSettings.UseNsdkSemanticSegmentation;
                        return
                            getIosIsLightshipPluginEnabled.Invoke() &&
                            isFeatureEnabled;
                    },
                    FixIt = () =>
                    {
                        PlayerSettings.iOS.targetOSVersionString = "13.0";
                    },
                    FixItMessage = "Open `Project Settings` > `Player` > `iOS settings` > `Other Settings` and set the target iOS version to 13.0 or higher.",
                    FixItAutomatic = true,
                    Error = true
                },
                new BuildValidationRule
                {
                    Category = Category,
                    Message = $"If using NSDK Scanning feature for iOS, set the target iOS version to 14.0 or higher (currently {getIosTargetOsVersionString.Invoke()}).",
                    CheckPredicate = () => OSVersion.Parse(getIosTargetOsVersionString.Invoke()) >= new OSVersion(14),
                    IsRuleEnabled = () =>
                    {
                        var isFeatureEnabled = nsdkSettings.UseNsdkScanning;
                        return
                            getIosIsLightshipPluginEnabled.Invoke() &&
                            isFeatureEnabled;
                    },
                    FixIt = () =>
                    {
                        PlayerSettings.iOS.targetOSVersionString = "14.0";
                    },
                    FixItMessage = "Open `Project Settings` > `Player` > `iOS settings` > `Other Settings` and set the target iOS version to 14.0 or higher.",
                    FixItAutomatic = true,
                    Error = true
                },
                new BuildValidationRule
                {
                    Category = Category,
                    Message = "If using NSDK VPS or Scanning features for iOS, set the location usage description.",
                    CheckPredicate = () => !string.IsNullOrEmpty(getIosLocationUsageDescription.Invoke()),
                    IsRuleEnabled = () =>
                    {
                        var isFeatureEnabled =
                            nsdkSettings.UseNsdkScanning ||
                            nsdkSettings.UseNsdkPersistentAnchor;
                        return
                            getIosIsLightshipPluginEnabled.Invoke() &&
                            isFeatureEnabled;
                    },
                    FixIt = () =>
                    {
                        PlayerSettings.iOS.locationUsageDescription = "Lightship VPS needs access to your location.";
                    },
                    FixItMessage = "Open 'Project Settings' > 'Player' > 'iOS Settings' > `Other Settings` and set the location usage description.",
                    FixItAutomatic = true,
                    Error = true
                }
            };
            return iOSRules;
        }

        internal static BuildValidationRule[] CreateAndroidRules(
            [NotNull] Func<bool> getAndroidIsLightshipPluginEnabled,
            [NotNull] Func<int> getAndroidTargetSdkVersion,
            [NotNull] Func<string> getAndroidGradleVersion,
            [NotNull] Func<BuildTarget> getActiveBuildTarget)
        {
            var androidRules = new[]
            {
#if !NIANTICSPATIAL_NSDK_META_ENABLED && !NIANTICSPATIAL_NSDK_ML2_ENABLED
                new BuildValidationRule
                {
                    Category = Category,
                    Message = "If using NSDK for Android, enable the 'Niantic Spatial Development Kit' plug-in.",
                    CheckPredicate = getAndroidIsLightshipPluginEnabled.Invoke,
                    FixIt = () =>
                    {
                        EditorApplication.ExecuteMenuItem("Lightship/XR Plug-in Management");
                    },
                    FixItMessage = "Open `Project Settings` > `XR Plug-in Management` > `Android settings` and enable the 'Niantic Spatial Development Kit' plug-in.",
                    FixItAutomatic = false,
                    Error = false,
                },
#endif
                new BuildValidationRule
                {
                    Category = Category,
                    Message = $"If using NSDK for Android, set the Android Gradle path to that of a Gradle of version {s_minGradleVersion} or higher.",
                    CheckPredicate = () => new Version(getAndroidGradleVersion.Invoke()) >= s_minGradleVersion,
                    IsRuleEnabled = () =>
                        getAndroidIsLightshipPluginEnabled.Invoke() &&
                        getActiveBuildTarget.Invoke() == BuildTarget.Android,
                    FixIt = () =>
                    {
                        SettingsService.OpenUserPreferences(PreferencesExternalToolsPath);
                    },
                    FixItMessage = $"Open `Preferences` > `External Tools` and set the Android Gradle path to that of a Gradle of version {s_minGradleVersion} or higher.",
                    FixItAutomatic = false,
                    HelpText = "For further assistance, follow the instructions in the Lightship SDK docs.",
                    HelpLink = UpdateGradleVersionHelpLink,
                    Error = true
                },
                new BuildValidationRule
                {
                    Category = Category,
                    Message = "If using NSDK for Android, set the graphics API to OpenGLES3.",
                    IsRuleEnabled = getAndroidIsLightshipPluginEnabled.Invoke,
                    CheckPredicate = () =>
                    {
                        var graphicsApis = PlayerSettings.GetGraphicsAPIs(BuildTarget.Android);
                        return graphicsApis.Length > 0 && graphicsApis[0] == GraphicsDeviceType.OpenGLES3;
                    },
                    FixItMessage = "Open `Project Settings` > `Player` > `Android setting` and disable 'Auto Graphics API' then set the graphics API to OpenGLES3",
                    FixIt = () =>
                    {
                        PlayerSettings.SetUseDefaultGraphicsAPIs(BuildTarget.Android, false);
                        var currentGraphicsApis = PlayerSettings.GetGraphicsAPIs(BuildTarget.Android);
                        GraphicsDeviceType[] correctGraphicsApis;
                        if (currentGraphicsApis.Length == 0)
                        {
                            correctGraphicsApis = new[]
                            {
                                GraphicsDeviceType.OpenGLES3
                            };
                        }
                        else
                        {
                            var graphicApis = new List<GraphicsDeviceType>(currentGraphicsApis.Length);
                            graphicApis.Add(GraphicsDeviceType.OpenGLES3);
                            foreach (var graphicsApi in currentGraphicsApis)
                            {
                                if (graphicsApi != GraphicsDeviceType.OpenGLES3)
                                {
                                    graphicApis.Add(graphicsApi);
                                }
                            }
                            correctGraphicsApis = graphicApis.ToArray();
                        }
                        PlayerSettings.SetGraphicsAPIs(BuildTarget.Android, correctGraphicsApis);
                    },
                    Error = true
                },
#if !UNITY_2022_1_OR_NEWER
                // This rule is only enabled on Unity versions 2022 or lower since Unity 2022 no longer has
                // a means to programatically set the android sdk version to 33 as it expects the
                // "highest installed version" option to be used
                new BuildValidationRule
                {
                    Category = Category,
                    Message = $"If using NSDK for Android, set the target Android SDK version to 33 or higher" +
                        $" (currently {(getAndroidTargetSdkVersion.Invoke() == 0 ? "automatic" : getAndroidTargetSdkVersion.Invoke())}).",
                    CheckPredicate = () => getAndroidTargetSdkVersion.Invoke() >= 33 || getAndroidTargetSdkVersion.Invoke() == 0,
                    IsRuleEnabled = getAndroidIsLightshipPluginEnabled.Invoke,
                    FixIt = () => PlayerSettings.Android.targetSdkVersion = (AndroidSdkVersions)33,
                    FixItMessage = $"Open `Project Settings` > `Player` > 'Android settings' > `Other Settings` and set the target Android SDK version to 33 or higher.",
                    FixItAutomatic = true,
                    Error = true
                }
#endif
            };
            return androidRules;
        }

        internal static BuildValidationRule[] CreateStandaloneRules(
            [NotNull] Func<bool> getStandaloneIsLightshipPluginEnabled,
            [NotNull] Func<bool> getSimulationPluginsStatusMatches)
        {
            var standaloneRules = new[]
            {
                new BuildValidationRule
                {
                    Category = Category,
                    Message = "If using NSDK for Standalone with a playback dataset, enable the 'Niantic Spatial Development Kit' plug-in.",
                    CheckPredicate = getStandaloneIsLightshipPluginEnabled.Invoke,
                    FixIt = () =>
                    {
                        var generalSettings = XRGeneralSettingsPerBuildTarget.XRGeneralSettingsForBuildTarget(BuildTargetGroup.Standalone);
                        if (null == generalSettings)
                            return;

                        var managerSettings = generalSettings.AssignedSettings;
                        if (null == managerSettings)
                            return;

                        XRPackageMetadataStore.AssignLoader(managerSettings, typeof(NsdkStandaloneLoader).FullName, BuildTargetGroup.Standalone);
                    },
                    FixItMessage = "Open `Project Settings` > `XR Plug-in Management` > `Standalone settings` and enable the 'Niantic Spatial Development Kit' plug-in.",
                    FixItAutomatic = true,
                    Error = false,
                },
                new BuildValidationRule
                {
                    Category = Category,
                    Message = "If using NSDK for Simulation, enable both the 'NSDK Simulation' and 'XR Simulation' plug-ins.",
                    CheckPredicate = getSimulationPluginsStatusMatches.Invoke,
                    FixIt = () =>
                    {
                        var generalSettings = XRGeneralSettingsPerBuildTarget.XRGeneralSettingsForBuildTarget(BuildTargetGroup.Standalone);
                        if (null == generalSettings)
                            return;

                        var managerSettings = generalSettings.AssignedSettings;
                        if (null == managerSettings)
                            return;

                        if (NsdkEditorUtilities.IsUnitySimulationPluginEnabled())
                        {
                            // NSDK Simulation Loader needs to be enabled and must take precedence
                            XRPackageMetadataStore.AssignLoader(managerSettings,
                                "NianticSpatial.NSDK.AR.Loader.NsdkSimulationLoader", BuildTargetGroup.Standalone);
                        }
                        else
                        {
                            // Unity's XR Simulation Loader needs to be added to the list but will not be used.
                            // This is to unlock the XR Simulation UI menus.
                            XRPackageMetadataStore.AssignLoader(managerSettings,
                                "UnityEngine.XR.Simulation.SimulationLoader", BuildTargetGroup.Standalone);
                        }
                    },
                    FixItMessage = "Open `Project Settings` > `XR Plug-in Management` > `Standalone settings` and enable " +
                        "both the 'NSDK Simulation' and 'XR Simulation' plug-ins.",
                    FixItAutomatic = true,
                    Error = false,
                },
            };
            return standaloneRules;
        }

        private static bool GetIosIsNsdkPluginEnabled()
        {
            return NsdkEditorUtilities.GetIosIsNsdkPluginEnabled();
        }

        private static bool GetAndroidIsNsdkPluginEnabled()
        {
            return NsdkEditorUtilities.GetAndroidIsNsdkPluginEnabled();
        }

        private static bool GetStandaloneIsNsdkPluginEnabled()
        {
            // Standalone mode is used for both playback and simulation.
            // If the NSDK Simulation plugin is enabled, don't display a warning about the standalone plugin.
            return NsdkEditorUtilities.GetStandaloneIsNsdkPluginEnabled()
                || NsdkEditorUtilities.GetSimulationIsNsdkPluginEnabled();
        }

        // When using simulation, both the NSDK and Unity simulation plugins must be enabled to unlock the XR Environment UI menus.
        private static bool GetSimulationPluginsStatusMatches()
        {
            return !(NsdkEditorUtilities.GetSimulationIsNsdkPluginEnabled() ^ NsdkEditorUtilities.IsUnitySimulationPluginEnabled());
        }

        private static string GetAndroidGradleVersion()
        {
            // Note: This Gradle API call only works if the target platform is set to Android before making this call
            return Gradle.TryGetVersion(out var gradleVersion, out var message) ? gradleVersion.ToString() : new Version(0, 0).ToString();
        }

        private static string GetUnityVersion()
        {
            return Regex.Replace(Application.unityVersion, "[A-Za-z ]", "");
        }

        private static BuildTarget GetActiveBuildTarget()
        {
            return EditorUserBuildSettings.activeBuildTarget;
        }

        private static string GetIosTargetOsVersionString()
        {
            return PlayerSettings.iOS.targetOSVersionString;
        }

        private static string GetIosLocationUsageDescription()
        {
            return PlayerSettings.iOS.locationUsageDescription;
        }

        private static int GetAndroidTargetSdkVersion()
        {
            return (int)PlayerSettings.Android.targetSdkVersion;
        }

#if MODULE_URP_ENABLED
        private static void ConfigureRendererFeatures(ScriptableRendererData rendererData)
        {
            // Implementation for configuring renderer features
            var arFeature = ScriptableObject.CreateInstance<ARBackgroundRendererFeature>();
            arFeature.name = "ARBackgroundRendererFeature";
            AssetDatabase.AddObjectToAsset(arFeature, rendererData);
            rendererData.rendererFeatures.Add(arFeature);
            EditorUtility.SetDirty(rendererData);
        }
        private static ScriptableRendererData CreateURPAssets()
        {
            // Create a default URP asset
            var urpAsset = ScriptableObject.CreateInstance<UniversalRenderPipelineAsset>();
            var urpPath = "Assets/DefaultUniversalRenderPipelineAsset.asset";
            AssetDatabase.CreateAsset(urpAsset, urpPath);
            // Create Universal Renderer asset
            var rendererData = ScriptableObject.CreateInstance<UniversalRendererData>();
            var rendererPath = "Assets/DefaultUniversalRenderer.asset";
            AssetDatabase.CreateAsset(rendererData, rendererPath);

            // Assign renderer to URP asset
            var rendererDataListField = urpAsset.GetType().GetField("m_RendererDataList", BindingFlags.NonPublic | BindingFlags.Instance);
            if (rendererDataListField != null)
            {
                rendererDataListField.SetValue(urpAsset, new ScriptableRendererData[] { rendererData });
            }

            // Set default renderer index (optional, usually 0)
            var defaultRendererIndexField = urpAsset.GetType().GetField("m_DefaultRendererIndex", BindingFlags.NonPublic | BindingFlags.Instance);
            if (defaultRendererIndexField != null)
            {
                defaultRendererIndexField.SetValue(urpAsset, 0);
            }
            GraphicsSettings.defaultRenderPipeline = urpAsset;
            EditorUtility.SetDirty(GraphicsSettings.GetGraphicsSettings());
            AssetDatabase.SaveAssets();

            return rendererData;
        }
        private static void FindNonBackgroundRendererData(Dictionary<string, ScriptableRendererData> nonBackgroundRendererDataList, string[] guids)
        {
            // Find all URP assets
            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var asset = AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>(path);
                if (asset != null)
                {
                    var rendererDataListField = typeof(UniversalRenderPipelineAsset).GetField("m_RendererDataList",
                        BindingFlags.NonPublic | BindingFlags.Instance);
                    if (rendererDataListField == null)
                    {
                        return;
                    }

                    var rendererDataList = rendererDataListField.GetValue(asset) as ScriptableRendererData[];
                    if (rendererDataList == null || rendererDataList.Length == 0)
                    {
                        return;
                    }

                    // Check all rendererData for those that don't have the ARBackgroundRendererFeature in them
                    foreach (var rendererData in rendererDataList)
                    {
                        if (rendererData == null ||
                        !rendererData.rendererFeatures.Any(feature => feature is ARBackgroundRendererFeature))
                        {
                            nonBackgroundRendererDataList[guid] = rendererData;
                        }
                    }
                }
            }
        }
#endif

    }
}
