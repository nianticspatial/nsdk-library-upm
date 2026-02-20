// Copyright 2022-2026 Niantic Spatial.

using System.Linq;
using NianticSpatial.NSDK.AR.Loader;
using UnityEditor;
using UnityEditor.XR.Management;
using UnityEditor.XR.Management.Metadata;
using UnityEngine;
using UnityEngine.XR.Simulation;

namespace NianticSpatial.NSDK.Simulation.Editor
{
    [XRCustomLoaderUI("NianticSpatial.NSDK.AR.Loader.NsdkSimulationLoader", BuildTargetGroup.Standalone)]
    internal class _NsdkAllLoaderUI : IXRCustomLoaderUI
    {
        private readonly struct Content
        {
            public static readonly GUIContent LoaderName = new GUIContent("Niantic Spatial NSDK Simulation");
        }

        public void SetRenderedLineHeight(float height)
        {
            RequiredRenderHeight = height;
        }

        public void OnGUI(Rect rect)
        {
            var size = EditorStyles.toggle.CalcSize(Content.LoaderName);
            var labelRect = new Rect(rect) { width = size.x, height = RequiredRenderHeight };
            IsLoaderEnabled = EditorGUI.ToggleLeft(labelRect, Content.LoaderName, IsLoaderEnabled);
        }

        private bool _isLoaderEnabled = false;
        public bool IsLoaderEnabled
        {
            get
            {
                return _isLoaderEnabled;
            }
            set
            {
                var loaderPreviouslyEnabled = _isLoaderEnabled;
                _isLoaderEnabled = value;
                // If NSDK Simulation is enabled, also keep Unity's XR Simulation loader enabled to unlock the
                // XR Environment UI menus.
                if (_isLoaderEnabled && !IsUnitySimulationPluginEnabled())
                {
                    var generalSettings = XRGeneralSettingsPerBuildTarget.XRGeneralSettingsForBuildTarget(BuildTargetGroup.Standalone);
                    if (null == generalSettings)
                        return;

                    var managerSettings = generalSettings.AssignedSettings;
                    if (null == managerSettings)
                        return;

                    // Unity's XR Simulation Loader needs to be added to the list but will not be used.
                    // This is to unlock the XR Simulation UI menus.
                    XRPackageMetadataStore.AssignLoader(managerSettings, typeof(SimulationLoader).FullName, BuildTargetGroup.Standalone);
                }
                else if (!_isLoaderEnabled && loaderPreviouslyEnabled && IsUnitySimulationPluginEnabled())
                {
                    var generalSettings =
                        XRGeneralSettingsPerBuildTarget.XRGeneralSettingsForBuildTarget(BuildTargetGroup.Standalone);
                    if (null == generalSettings)
                        return;

                    var managerSettings = generalSettings.AssignedSettings;
                    if (null == managerSettings)
                        return;

                    // Uncheck the XR Simulation loader too.
                    XRPackageMetadataStore.RemoveLoader(managerSettings, typeof(SimulationLoader).FullName, BuildTargetGroup.Standalone);
                }
            }
        }

        /**
         * List of incompatible Loaders which will be disabled when NSDK is enabled in XR-Plugin Management.
         * Strongly typed loader references using typeof() is preferred, but any package
         * in XR-Plugin Management/Editor/Metadata/KnownPackages.cs must be referenced using hard strings.
         */
        public string[] IncompatibleLoaders => new[]
        {
            typeof(NsdkStandaloneLoader).FullName,
            typeof(UnityEngine.XR.ARCore.ARCoreLoader).FullName,
            typeof(UnityEngine.XR.ARKit.ARKitLoader).FullName,
            "Unity.XR.Oculus.OculusLoader",
            "UnityEngine.XR.OpenXR.OpenXRLoader",
            "Unity.XR.MockHMD.MockHMDLoader",
            "UnityEngine.XR.WindowsMR.WindowsMRLoader",
            "UnityEngine.XR.MagicLeap.MagicLeapLoader",
            "NianticSpatial.NSDK.AR.Loader.NsdkSimulationLoader"
        };

        public float RequiredRenderHeight { get; private set; }
        public BuildTargetGroup ActiveBuildTargetGroup { get; set; }

        private static bool IsUnitySimulationPluginEnabled()
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
