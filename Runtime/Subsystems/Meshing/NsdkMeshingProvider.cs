// Copyright 2022-2026 Niantic Spatial.

using System;
using System.Runtime.InteropServices;
using NianticSpatial.NSDK.AR.Core;

namespace NianticSpatial.NSDK.AR.Subsystems.Meshing
{
    internal class NsdkMeshingProvider
    {
        private static bool s_isConstructed;

        public static void Construct(IntPtr unityContext)
        {
            if (!s_isConstructed)
            {
                Lightship_ARDK_Unity_Meshing_Provider_Construct(unityContext);
                s_isConstructed = true;
            }
        }

        public static void Reset()
        {
            // Reset the construction flag so that the next loader initialization will call
            // Construct() again. This is necessary because Construct() registers Unity's meshing
            // lifecycle callbacks (Initialize/Start/Stop/Shutdown) and creates the native
            // MeshingProvider singleton via GetInstance().reset(new MeshingProvider(...)). If
            // s_isConstructed is not cleared, subsequent loader initializations skip Construct(),
            // leaving the native singleton in its post-Shutdown state (ardk_handle_ == nullptr),
            // which causes ARDK_Status_NullArdkHandle errors when Unity starts the subsystem.
            // There is no native Destroy API — the native lifecycle is fully managed through
            // Unity's subsystem callbacks, with Shutdown() handling native cleanup. Re-calling
            // Construct() after Shutdown() is safe: it replaces the singleton via unique_ptr::reset
            // and re-registers the lifecycle provider.
            s_isConstructed = false;
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
