// Copyright 2022-2026 Niantic Spatial.

using System.Collections.Generic;
using NianticSpatial.NSDK.AR.Utilities.Logging;
using NianticSpatial.NSDK.AR.Loader;
using UnityEngine;
using UnityEditor;
using UnityEditor.XR.Management.Metadata;

namespace NianticSpatial.NSDK.AR.Editor
{
    internal class XRPackage : IXRPackage
    {
        private class NsdkLoaderMetadata : IXRLoaderMetadata
        {
            public string loaderName { get; set; }
            public string loaderType { get; set; }
            public List<BuildTargetGroup> supportedBuildTargets { get; set; }
        }

        private class NsdkPackageMetadata : IXRPackageMetadata
        {
            public string packageName { get; set; }
            public string packageId { get; set; }
            public string settingsType { get; set; }
            public List<IXRLoaderMetadata> loaderMetadata { get; set; }
        }

        private static IXRPackageMetadata s_Metadata = new NsdkPackageMetadata
        {
            packageName = NsdkPackageInfo.displayName,
            packageId = NsdkPackageInfo.identifier,
            settingsType = typeof(NsdkSettings).FullName,
            loaderMetadata = new List<IXRLoaderMetadata>
            {
                new NsdkLoaderMetadata
                {
                    loaderName = "Niantic Spatial Development Kit for Unity Editor",
                    loaderType = typeof(NsdkStandaloneLoader).FullName,
                    supportedBuildTargets = new List<BuildTargetGroup> { BuildTargetGroup.Standalone }
                },
                new NsdkLoaderMetadata
                {
                    loaderName = "Niantic Spatial Development Kit Simulation",
                    loaderType = typeof(NsdkSimulationLoader).FullName,
                    supportedBuildTargets = new List<BuildTargetGroup>() { BuildTargetGroup.Standalone, }
                },

                new NsdkLoaderMetadata()
                {
                    loaderName = "Niantic Spatial Development Kit + Google ARCore",
                    loaderType = typeof(NsdkARCoreLoader).FullName,
                    supportedBuildTargets = new List<BuildTargetGroup> { BuildTargetGroup.Android }
                },
                new NsdkLoaderMetadata
                {
                    loaderName = "Niantic Spatial Development Kit + Apple ARKit",
                    loaderType = typeof(NsdkARKitLoader).FullName,
                    supportedBuildTargets = new List<BuildTargetGroup> { BuildTargetGroup.iOS }
                }
            }
        };

        public IXRPackageMetadata metadata => s_Metadata;

        public bool PopulateNewSettingsInstance(ScriptableObject obj)
        {
            try
            {
                EditorBuildSettings.AddConfigObject(NsdkSettings.SettingsKey, obj, true);
                return true;
            }
            catch (System.Exception ex)
            {
                Log.Info($"Error adding new NSDK Settings object to build settings.\n{ex.Message}");
            }

            return false;
        }
    }
}
