// Copyright 2022-2026 Niantic Spatial.
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace NianticSpatial.NSDK.AR.Simulation
{
    internal static class NsdkSimulationEditorUtility
    {
        public static float GetGameViewAspectRatio()
        {
#if UNITY_EDITOR
            return CameraEditorUtils.GameViewAspectRatio;
#else
            return 1.0f;
#endif
        }
    }
}
