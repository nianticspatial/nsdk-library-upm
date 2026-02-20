// Copyright 2022-2026 Niantic Spatial.

using System;
using System.Linq;
using NianticSpatial.NSDK.AR.Loader;
using UnityEditor;
using UnityEditor.XR.Management;
using UnityEngine.XR.Simulation;

namespace NianticSpatial.NSDK.AR.Editor
{
    internal static class NsdkEditorUtilities
    {
        internal static bool GetIosIsNsdkPluginEnabled()
        {
            return GetIsNsdkPluginEnabledForPlatform(BuildTargetGroup.iOS, typeof(NsdkARKitLoader));
        }

        internal static bool GetAndroidIsNsdkPluginEnabled()
        {
            return GetIsNsdkPluginEnabledForPlatform(BuildTargetGroup.Android, typeof(NsdkARCoreLoader));
        }

        internal static bool GetStandaloneIsNsdkPluginEnabled()
        {
            return GetIsNsdkPluginEnabledForPlatform(BuildTargetGroup.Standalone, typeof(NsdkStandaloneLoader));
        }

        internal static bool GetSimulationIsNsdkPluginEnabled()
        {
            return GetIsNsdkPluginEnabledForPlatform(BuildTargetGroup.Standalone, typeof(NsdkSimulationLoader));
        }

        private static bool GetIsNsdkPluginEnabledForPlatform(BuildTargetGroup buildTargetGroup, Type nsdkLoaderType)
        {
            var generalSettings = XRGeneralSettingsPerBuildTarget.XRGeneralSettingsForBuildTarget(buildTargetGroup);
            if (generalSettings == null)
            {
                return false;
            }
            var managerSettings = generalSettings.AssignedSettings;
            var doesLoaderOfTypeExist = false;
            if (nsdkLoaderType == typeof(NsdkARKitLoader))
            {
                doesLoaderOfTypeExist = managerSettings.activeLoaders.Any(loader => loader is NsdkARKitLoader);
            }
            else if (nsdkLoaderType == typeof(NsdkARCoreLoader))
            {
                doesLoaderOfTypeExist = managerSettings.activeLoaders.Any(loader => loader is NsdkARCoreLoader);
            }
            else if (nsdkLoaderType == typeof(NsdkStandaloneLoader))
            {
                doesLoaderOfTypeExist = managerSettings.activeLoaders.Any(loader => loader is NsdkStandaloneLoader);
            }
            else if (nsdkLoaderType == typeof(NsdkSimulationLoader))
            {
                doesLoaderOfTypeExist = managerSettings.activeLoaders.Any(loader => loader is NsdkSimulationLoader);
            }
            return managerSettings != null && doesLoaderOfTypeExist;
        }

        internal static bool IsUnitySimulationPluginEnabled()
        {
            var generalSettings = XRGeneralSettingsPerBuildTarget.XRGeneralSettingsForBuildTarget(BuildTargetGroup.Standalone);
            if (null == generalSettings)
                return false;

            var managerSettings = generalSettings.AssignedSettings;
            if (null == managerSettings)
                return false;

            var simulationLoaderIsActive = managerSettings.activeLoaders.Any(loader => loader is SimulationLoader);
            return simulationLoaderIsActive;
        }
    }
}
