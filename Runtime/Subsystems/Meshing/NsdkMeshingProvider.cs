// Copyright 2022-2026 Niantic Spatial.

using System;
using System.Runtime.InteropServices;
using NianticSpatial.NSDK.AR.Core;

namespace NianticSpatial.NSDK.AR.Subsystems.Meshing
{
    internal class NsdkMeshingProvider
    {
        private static bool s_isConstructed;

        [Obsolete("Use Construct(IntPtr unityContext) instead.")]
        public NsdkMeshingProvider(IntPtr unityContext)
        {
            Lightship_ARDK_Unity_Meshing_Provider_Construct(unityContext);
        }

        public static void Construct(IntPtr unityContext)
        {
            if (!s_isConstructed)
            {
                Lightship_ARDK_Unity_Meshing_Provider_Construct(unityContext);
                s_isConstructed = true;
            }
        }

        public static bool Configure
        (
            int frameRate,
            bool fuseKeyframesOnly,
            float maximumIntegrationDistance,
            float voxelSize,
            bool enableDistanceBasedVolumetricCleanup,
            int numVoxelLevels,
            float meshBlockSize,
            float meshCullingDistance,
            bool enableMeshDecimation,
            bool enableMeshFiltering,
            bool enableFilteringAllowList,
            int packedAllowList,
            bool enableFilteringBlockList,
            int packedBlockList
        )
        {
            return Lightship_ARDK_Unity_Meshing_Provider_Configure
            (
                frameRate,
                fuseKeyframesOnly,
                maximumIntegrationDistance,
                voxelSize,
                enableDistanceBasedVolumetricCleanup,
                numVoxelLevels,
                meshBlockSize,
                meshCullingDistance,
                enableMeshDecimation,
                enableMeshFiltering,
                enableFilteringAllowList,
                packedAllowList,
                enableFilteringBlockList,
                packedBlockList
            );
        }

        public static ulong GetLastMeshUpdateTime()
        {
            return Lightship_ARDK_Unity_Meshing_Provider_LatestTimestamp();
        }

        [DllImport(NsdkPlugin.Name)]
        private static extern IntPtr Lightship_ARDK_Unity_Meshing_Provider_Construct(IntPtr unityContext);

        [DllImport(NsdkPlugin.Name)]
        private static extern ulong Lightship_ARDK_Unity_Meshing_Provider_LatestTimestamp();

        [DllImport(NsdkPlugin.Name)]
        private static extern bool Lightship_ARDK_Unity_Meshing_Provider_Configure
        (
            int frameRate,
            bool fuseKeyframesOnly,
            float maximumIntegrationDistance,
            float voxelSize,
            bool enableDistanceBasedVolumetricCleanup,
            int numVoxelLevels,
            float meshBlockSize,
            float meshCullingDistance,
            bool enableMeshDecimation,
            bool enableMeshFiltering,
            bool enableFilteringAllowList,
            int packedAllowList,
            bool enableFilteringBlockList,
            int packedBlockList
        );
    }
}
