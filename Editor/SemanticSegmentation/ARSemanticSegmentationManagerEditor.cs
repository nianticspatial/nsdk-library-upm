// Copyright 2022-2026 Niantic Spatial.

using NianticSpatial.NSDK.AR.Semantics;
using NianticSpatial.NSDK.AR.Subsystems.Occlusion;
using UnityEditor;
using UnityEngine;

namespace NianticSpatial.NSDK.AR.Editor.SemanticSegmentation
{
    [CustomEditor(typeof(ARSemanticSegmentationManager))]
    internal class ARSemanticSegmentationManagerEditor : UnityEditor.Editor
    {
        private SerializedProperty _targetFrameRate;

        private static class Tooltips
        {
            public const string HighFrameRateWarning = "A target framerate over 20 could negatively affect performance on older devices.";

            public static readonly GUIContent TargetFrameRate = new GUIContent
                ("Target Frame Rate", "Frame rate that semantic segmentation inference will aim to run at");

        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(_targetFrameRate, Tooltips.TargetFrameRate);
            if (_targetFrameRate.intValue > NsdkOcclusionSubsystem.MaxRecommendedFrameRate)
            {
                EditorGUILayout.HelpBox(Tooltips.HighFrameRateWarning, MessageType.Warning);
            }

            serializedObject.ApplyModifiedProperties();
        }

        private void OnEnable()
        {
            _targetFrameRate = serializedObject.FindProperty("_targetFrameRate");
        }
    }
}
