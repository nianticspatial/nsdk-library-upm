// Copyright 2022-2025 Niantic.

using System;
using System.Collections;
using System.IO;
using NianticSpatial.NSDK.AR.Loader;
using NianticSpatial.NSDK.AR.Subsystems.Playback;
using Unity.EditorCoroutines.Editor;
using UnityEditor;
using UnityEngine;

namespace NianticSpatial.NSDK.AR.Editor
{
    public abstract class BasePlaybackSettingsEditor : IPlaybackSettingsEditor
    {
        private static class Contents
        {
            public static readonly GUIContent UsePlaybackLabel =
                new GUIContent
                (
                    "Enabled",
                    "When enabled, a dataset will be used to simulate live AR input."
                );

            public static readonly GUIContent DatasetPathLabel =
                new GUIContent
                (
                    "Dataset Path",
                    "The absolute path to the folder containing the dataset to use for playback in-Editor. " +
                    "Does not need to be within the Unity project directory."
                );

            public static readonly GUIContent RunManuallyLabel =
                new GUIContent
                (
                    "Run Manually",
                    "Use the space bar to move forward frame by frame."
                );

            public static readonly GUIContent LoopInfinitelyLabel =
                new GUIContent
                (
                    "Loop Infinitely",
                    "When enabled, the playback dataset will play continuously rather than halt at the end. " +
                    "To prevent immediate jumps in pose and tracking, the dataset will alternate running forwards and backwards."
                );
        }

        protected abstract INsdkPlaybackSettings PlaybackSettings { get; }

        /// <summary>
        /// Marks the underlying ScriptableObject as dirty to ensure changes are saved
        /// </summary>
        protected virtual void MarkSettingsDirty()
        {
            EditorUtility.SetDirty(NsdkSettings.Instance);
        }

        private PlaybackDataset _dataset;
        private Texture2D[] _frameTextures;
        private bool _previewTexturesLoaded;
        private bool _allTexturesLoaded;

        private bool _onlyLoadPreviews = false;

        private const int PreviewLengthThreshold = 1800;
        private const int PreviewFrameCount = 24;
        private int FrameCount
        {
            get
            {
                if (_frameTextures != null)
                    return _frameTextures.Length;
                else
                    return PreviewFrameCount;
            }
        }

        private float _loadProgress = 0.0f;

        private EditorCoroutine _loadFrameTexturesCoroutine;



        private bool _watchingPlayMode;

        /// <summary>Stops preview loading and releases the native dataset handle.</summary>
        internal void Release()
        {
            if (_watchingPlayMode)
            {
                EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
                _watchingPlayMode = false;
            }

            CloseDataset();
        }

        private void CloseDataset()
        {
            if (_loadFrameTexturesCoroutine != null)
            {
                EditorCoroutineUtility.StopCoroutine(_loadFrameTexturesCoroutine);
                _loadFrameTexturesCoroutine = null;
            }

            _dataset?.Close();
            _dataset = null;
        }

        // Unity destroys textures created during play mode when it exits; reopening rebuilds them.
        private void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.EnteredEditMode)
            {
                CloseDataset();
            }
        }

        public void DrawGUI()
        {
            if (!_watchingPlayMode)
            {
                EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
                _watchingPlayMode = true;
            }

            var currUsedPlayback = PlaybackSettings.UsePlayback;
            var newUsePlayback = EditorGUILayout.Toggle(Contents.UsePlaybackLabel, currUsedPlayback);
            if (newUsePlayback != currUsedPlayback)
            {
                PlaybackSettings.UsePlayback = newUsePlayback;
                MarkSettingsDirty();
            }

            EditorGUI.BeginDisabledGroup(!newUsePlayback);
            {
                var wasPathUpdated = DrawDatasetPathGUI();

                if (string.IsNullOrEmpty(PlaybackSettings.PlaybackDatasetPath))
                {
                    _dataset?.Close();
                    _dataset = null;
                    _previewTexturesLoaded = false;
                    _allTexturesLoaded = false;
                    _onlyLoadPreviews = false;
                    _frameTextures = null;
                }
                else if (_dataset == null || wasPathUpdated)
                {
                    _dataset?.Close();
                    _dataset = PlaybackDataset.Open(PlaybackSettings.PlaybackDatasetPath);
                    _previewTexturesLoaded = false;
                    _allTexturesLoaded = false;
                    _onlyLoadPreviews = false;
                    _frameTextures = null;
                    if (_loadFrameTexturesCoroutine != null)
                    {
                        EditorCoroutineUtility.StopCoroutine(_loadFrameTexturesCoroutine);
                    }

                    _loadFrameTexturesCoroutine = EditorCoroutineUtility.StartCoroutine(LoadFrameTextures(), this);
                }

                if (_dataset != null)
                {
                    DrawTrimToolGUI(wasPathUpdated);
                }
                else
                {
                    EditorGUILayout.HelpBox("Please enter a valid Playback dataset path", MessageType.Warning);
                }

                var currRunManually = PlaybackSettings.RunManually;
                var changedRunManually = EditorGUILayout.Toggle(Contents.RunManuallyLabel, currRunManually);
                if (changedRunManually != currRunManually)
                {
                    PlaybackSettings.RunManually = changedRunManually;
                    MarkSettingsDirty();
                }

                var currLoopInfinitely = PlaybackSettings.LoopInfinitely;
                var changedLoopInfinitely = EditorGUILayout.Toggle(Contents.LoopInfinitelyLabel, currLoopInfinitely);
                if (changedLoopInfinitely != currLoopInfinitely)
                {
                    PlaybackSettings.LoopInfinitely = changedLoopInfinitely;
                    MarkSettingsDirty();
                }
            }
            EditorGUI.EndDisabledGroup();
        }

        private bool DrawDatasetPathGUI()
        {
            EditorGUILayout.BeginHorizontal();

            var currPath = PlaybackSettings.PlaybackDatasetPath;
            var changedPath = EditorGUILayout.TextField(Contents.DatasetPathLabel, currPath);
            var wasPathUpdated = false;

            if (changedPath != currPath)
            {
                PlaybackSettings.PlaybackDatasetPath = changedPath;
                MarkSettingsDirty();
                wasPathUpdated = true;
            }

            var browse = GUILayout.Button("Browse", GUILayout.Width(125));

            EditorGUILayout.EndHorizontal();

            if (browse)
            {
                var browsedPath =
                    EditorUtility.OpenFolderPanel
                    (
                        "Select Dataset Directory",
                        changedPath,
                        ""
                    );

                if (!browsedPath.Equals(currPath))
                {
                    if (browsedPath.Length > 0)
                    {
                        PlaybackSettings.PlaybackDatasetPath = browsedPath;
                        MarkSettingsDirty();
                    }

                    wasPathUpdated = true;
                }
            }

            return wasPathUpdated;
        }

        private const float TimelineHeight = 72f;
        private const float TimelinePadding = 4f;

        private void DrawTrimToolGUI(bool wasPathUpdated)
        {
            var maxFrame = _dataset.FrameCount - 1;
            if (maxFrame < 1)
            {
                return;
            }

            int startFrame;
            int endFrame;
            if (wasPathUpdated)
            {
                startFrame = 0;
                endFrame = maxFrame;
            }
            else
            {
                startFrame = PlaybackSettings.StartFrame;
                endFrame = PlaybackSettings.EndFrame < 0 ? maxFrame : PlaybackSettings.EndFrame;
            }

            startFrame = Mathf.Clamp(startFrame, 0, maxFrame - 1);
            endFrame = Mathf.Clamp(endFrame, startFrame + 1, maxFrame);

            EditorGUILayout.Space(4);

            // Timeline, shading and slider share one rect so frame positions line up.
            var timeline = GUILayoutUtility.GetRect(0, TimelineHeight, GUILayout.ExpandWidth(true));
            if (Event.current.type == EventType.Repaint)
            {
                EditorGUI.DrawRect(timeline, new Color(0.15f, 0.15f, 0.15f, 1f));
                DrawThumbnails(timeline, maxFrame);

                var shade = new Color(0f, 0f, 0f, 0.6f);
                var startX = FrameToX(timeline, startFrame, maxFrame);
                var endX = FrameToX(timeline, endFrame, maxFrame);
                EditorGUI.DrawRect(new Rect(timeline.x, timeline.y, startX - timeline.x, timeline.height), shade);
                EditorGUI.DrawRect(new Rect(endX, timeline.y, timeline.xMax - endX, timeline.height), shade);

                var marker = new Color(1f, 0.8f, 0.2f, 1f);
                EditorGUI.DrawRect(new Rect(startX - 1, timeline.y, 2, timeline.height), marker);
                EditorGUI.DrawRect(new Rect(endX - 1, timeline.y, 2, timeline.height), marker);
            }

            var sliderRect = GUILayoutUtility.GetRect(0, EditorGUIUtility.singleLineHeight, GUILayout.ExpandWidth(true));
            float sliderStart = startFrame;
            float sliderEnd = endFrame;
            EditorGUI.MinMaxSlider(sliderRect, ref sliderStart, ref sliderEnd, 0, maxFrame);
            if (!Mathf.Approximately(sliderStart, startFrame) || !Mathf.Approximately(sliderEnd, endFrame))
            {
                startFrame = Mathf.RoundToInt(sliderStart);
                endFrame = Mathf.RoundToInt(sliderEnd);
            }

            startFrame = Mathf.Clamp(startFrame, 0, maxFrame - 1);
            endFrame = Mathf.Clamp(endFrame, startFrame + 1, maxFrame);

            var duration = _dataset.Frames[endFrame].TimestampInSeconds - _dataset.Frames[startFrame].TimestampInSeconds;
            EditorGUILayout.LabelField(
                $"Playing frames {startFrame}–{endFrame} of {_dataset.FrameCount} ({duration:F1} s)",
                EditorStyles.miniLabel);

            if (!_allTexturesLoaded)
            {
                EditorGUILayout.HelpBox($"Loading playback dataset... ({_loadProgress:F2}%)", MessageType.Info);
            }

            EditorGUILayout.Space(6);

            if (startFrame != PlaybackSettings.StartFrame || endFrame != PlaybackSettings.EndFrame)
            {
                PlaybackSettings.StartFrame = startFrame;
                PlaybackSettings.EndFrame = endFrame;
                MarkSettingsDirty();
            }
        }

        private static float FrameToX(Rect timeline, int frame, int maxFrame)
        {
            return timeline.x + timeline.width * frame / maxFrame;
        }

        private void DrawThumbnails(Rect timeline, int maxFrame)
        {
            if (_frameTextures == null)
            {
                return;
            }

            var thumbHeight = timeline.height - 2 * TimelinePadding;
            var aspect = ThumbnailAspect();
            var thumbWidth = thumbHeight * aspect;
            var slots = Mathf.Clamp(Mathf.FloorToInt(timeline.width / thumbWidth), 1, _dataset.FrameCount);
            var slotWidth = timeline.width / slots;

            for (var slot = 0; slot < slots; slot++)
            {
                var frame = slots == 1 ? 0 : Mathf.RoundToInt((float)slot * maxFrame / (slots - 1));
                var texture = _frameTextures[GetTextureFrame(frame)];
                if (texture == null)
                {
                    continue;
                }

                var rect = new Rect(
                    timeline.x + slot * slotWidth + (slotWidth - thumbWidth) / 2,
                    timeline.y + TimelinePadding,
                    thumbWidth,
                    thumbHeight);
                GUI.DrawTexture(rect, texture, ScaleMode.ScaleToFit);
            }
        }

        private float ThumbnailAspect()
        {
            foreach (var texture in _frameTextures)
            {
                if (texture != null)
                {
                    return (float)texture.width / texture.height;
                }
            }

            return 1f;
        }

        private void LoadFrameImage(int datasetIndex)
        {
            var frame = _dataset.Frames[datasetIndex];
            byte[] fileData = _dataset.ReadImage(datasetIndex);

            ProcessFrameTexture(fileData, frame, datasetIndex);
        }


        private void ProcessFrameTexture(byte[] fileData, PlaybackDataset.FrameMetadata frame, int datasetIndex)
        {
            Texture2D texture = new Texture2D(2, 2);
            texture.LoadImage(fileData);

            float rotationAngle = 0f;
            int width = texture.width;
            int height = texture.height;

            switch (frame.Orientation)
            {
                case ScreenOrientation.Portrait:
                    rotationAngle = Mathf.PI / 2; // 90 degrees
                    width = texture.height;
                    height = texture.width;
                    break;
                case ScreenOrientation.PortraitUpsideDown:
                    rotationAngle = 3 * Mathf.PI / 2; // 270 degrees
                    width = texture.height;
                    height = texture.width;
                    break;
                case ScreenOrientation.LandscapeLeft:
                    rotationAngle = 0f;
                    break;
                case ScreenOrientation.LandscapeRight:
                    rotationAngle = Mathf.PI; // 180 degrees
                    break;
                case ScreenOrientation.AutoRotation:
                case ScreenOrientation.Unknown:
                default:
                    break;
            }

            float aspectRatio = (float)width / height;
            height = 64;
            width = Mathf.RoundToInt(height * aspectRatio);

            RenderTexture rt = new RenderTexture(width, height, 0);
            Material mat = new Material(Shader.Find("Nsdk/Editor/Custom/RotateTextureShader"));
            mat.SetFloat("_Rotation", rotationAngle);
            Graphics.Blit(texture, rt, mat);

            RenderTexture.active = rt;
            Texture2D rotatedTexture = new Texture2D(width, height);
            rotatedTexture.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
            rotatedTexture.Apply();
            RenderTexture.active = null;

            _frameTextures[GetTextureFrame(datasetIndex)] = rotatedTexture;
        }

        private IEnumerator LoadAllFrames()
        {
            // Load all textures
            for (int i = 0; i < _dataset.FrameCount; i++)
            {
                if (_frameTextures[i] != null)
                {
                    continue;
                }

                LoadFrameImage(i);

                _loadProgress = (float)i / _dataset.FrameCount * 100.0f;

                if (i % 5 == 0)
                {
                    yield return null;
                }
            }
        }

        private void LoadPreviewFrames()
        {
            var framesToSkip = _dataset.FrameCount <= PreviewFrameCount ?
                1 :
                _dataset.FrameCount / PreviewFrameCount;

            // Load preview textures
            for (int i = 0; i < _dataset.FrameCount; i += framesToSkip)
            {
                LoadFrameImage(i);
            }
        }

        private IEnumerator LoadFrameTextures()
        {
            if (_dataset == null)
            {
                yield break;
            }

            if (_frameTextures != null)
            {
                yield break;
            }

            // If the dataset is too long, only load the previews
            int frameCount = _dataset.FrameCount;
            if (_dataset.FrameCount > PreviewLengthThreshold)
            {
                _onlyLoadPreviews = true;
                frameCount = PreviewFrameCount;
            }

            _frameTextures = new Texture2D[frameCount];

            LoadPreviewFrames();

            _previewTexturesLoaded = true;

            if (!_onlyLoadPreviews)
                yield return LoadAllFrames();

            _allTexturesLoaded = true;
        }

        private int GetTextureFrame(int datasetFrame)
        {
            if (!_onlyLoadPreviews)
                return datasetFrame;

            var framesToSkip = _dataset.FrameCount <= PreviewFrameCount ?
                1 :
                _dataset.FrameCount / PreviewFrameCount;

            // If dataset.FrameCount doesn't evenly divide by PreviewFrameCount, ensure the remainder corresponds to the final preview frame
            return Math.Clamp((datasetFrame / framesToSkip), 0, PreviewFrameCount - 1);
        }
    }
}
