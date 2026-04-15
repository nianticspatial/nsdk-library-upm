// Copyright 2022-2026 Niantic Spatial.

using System;
using NianticSpatial.NSDK.AR.Subsystems;
using NianticSpatial.NSDK.AR.Subsystems.Meshing;
using NianticSpatial.NSDK.AR.Subsystems.Occlusion;
using NianticSpatial.NSDK.AR.Subsystems.SceneSegmentation;
using NianticSpatial.NSDK.AR.Utilities.Logging;
using NianticSpatial.NSDK.AR.XRSubsystems;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.XR;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.XR.Management;
using Object = UnityEngine.Object;

namespace NianticSpatial.NSDK.AR.Utilities.Metrics
{
    internal class FPSMetricsUtility
    {
        private ARCameraManager _cameraManager;

        private XROcclusionSubsystem _occlusionSubsystem;
        private NsdkSceneSegmentationSubsystem _sceneSegmentationSubsystem;
        private NsdkMeshingProvider _meshingProvider;

        private bool _automaticallyTrackFPS;
        private bool _canAutomaticallyTrackFPS;

        private bool _usingDepth;
        private bool _usingSceneSegmentation;
        private bool _usingMesh;

        private ulong _lastTimeDepth;
        private ulong _lastTimeSceneSegmentation;
        private ulong _lastTimeMesh;

        private float _instantDepthFPS;
        private float _instantSceneSegmentationFPS;
        private float _instantMeshFPS;

        private uint? _latestSceneSegmentationFrameId;

        public FPSMetricsUtility(
            bool usingDepth = true,
            bool usingSceneSegmentation = true,
            bool usingMesh = true,
            bool automaticallyTrackFPS = true)
        {
            _usingDepth = usingDepth;
            _usingSceneSegmentation = usingSceneSegmentation;
            _usingMesh = usingMesh;
            _automaticallyTrackFPS = automaticallyTrackFPS;

            var xrManager = XRGeneralSettings.Instance?.Manager;
            if (xrManager == null || !xrManager.isInitializationComplete)
            {
                Log.Warning("XRManager is not initialized yet: cannot get subsystems");
                return;
            }

            _occlusionSubsystem = xrManager.activeLoader.GetLoadedSubsystem<XROcclusionSubsystem>();
            if (_occlusionSubsystem is null)
            {
                Log.Debug("Depth FPS not being tracked");
                _usingDepth = false;
            }

            _sceneSegmentationSubsystem = xrManager.activeLoader.GetLoadedSubsystem<XRSceneSegmentationSubsystem>() as NsdkSceneSegmentationSubsystem;
            if (_sceneSegmentationSubsystem is null)
            {
                Log.Debug("Scene Segmentation FPS not being tracked");
                _usingSceneSegmentation = false;
            }

            var activeMeshSubsystem = xrManager.activeLoader.GetLoadedSubsystem<XRMeshSubsystem>();
            if (activeMeshSubsystem is null || activeMeshSubsystem.SubsystemDescriptor.id != "LightshipMeshing")
            {
                Log.Debug("Mesh FPS not being tracked");
                _usingMesh = false;
            }

            TryAutomaticallyTrackFPS();
        }

        public void Dispose()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            if (_cameraManager != null)
            {
                _cameraManager.frameReceived -= OnFrameReceived;
            }
        }

        private void TryAutomaticallyTrackFPS()
        {
            if (_automaticallyTrackFPS)
            {
                // Check if we can actually track the fps using the ARCameraManager frameReceived event
                _cameraManager = Object.FindObjectOfType<ARCameraManager>(includeInactive: true);

                if (_cameraManager == null)
                {
                    Log.Warning("Cannot track FPS: No ARCameraManager found in scene");
                    _canAutomaticallyTrackFPS = false;
                    SceneManager.sceneLoaded += OnSceneLoaded;
                    return;
                }

                _canAutomaticallyTrackFPS = true;
                _cameraManager.frameReceived += OnFrameReceived;

                // Check if we can actually track the depth fps using the AROcclusionManager
                var arOcclusionManager = Object.FindObjectOfType<AROcclusionManager>(includeInactive: true);
                if (_usingDepth && arOcclusionManager == null)
                {
                    Log.Debug("Cannot track depth FPS: No AROcclusionManager found in scene");
                    _usingDepth = false;
                    return;
                }

                if (_usingDepth && arOcclusionManager.currentOcclusionPreferenceMode !=
                    OcclusionPreferenceMode.PreferEnvironmentOcclusion)
                {
                    Log.Debug("Cannot track depth FPS: " +
                        "AROcclusionManager is not set to PreferEnvironmentOcclusion");
                    _usingDepth = false;
                }
            }
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode loadSceneMode)
        {
            if (_cameraManager is not null)
            {
                _cameraManager.frameReceived -= OnFrameReceived;
            }
            SceneManager.sceneLoaded -= OnSceneLoaded;
            TryAutomaticallyTrackFPS();
        }

        private void OnFrameReceived(ARCameraFrameEventArgs args)
        {
            if (_usingDepth)
            {
                var thisTimeDepth = GetLatestDepthTimestamp();
                if (_usingDepth && thisTimeDepth != _lastTimeDepth)
                {
                    _instantDepthFPS = (1.0f / (Math.Abs((long)(thisTimeDepth - _lastTimeDepth)) / 1000.0f));
                    _lastTimeDepth = thisTimeDepth;
                }
            }

            if (_usingSceneSegmentation)
            {
                var thisTimeSceneSegmentation = GetLatestSceneSegmentationTimestamp();
                if (_usingSceneSegmentation && thisTimeSceneSegmentation != _lastTimeSceneSegmentation)
                {
                    _instantSceneSegmentationFPS = (1.0f / (Math.Abs((long)(thisTimeSceneSegmentation - _lastTimeSceneSegmentation)) / 1000.0f));
                    _lastTimeSceneSegmentation = thisTimeSceneSegmentation;
                }
            }

            if (_usingMesh)
            {
                var thisTimeMesh = GetLatestMeshTimestamp();
                if (_usingMesh && thisTimeMesh != _lastTimeMesh)
                {
                    _instantMeshFPS = (1.0f / (Math.Abs((long)(thisTimeMesh - _lastTimeMesh)) / 1000.0f));
                    _lastTimeMesh = thisTimeMesh;
                }
            }

        }

        public ulong GetLatestDepthTimestamp()
        {
            ulong depthTimestampMs = 0;

            if (!_usingDepth)
            {
                return depthTimestampMs;
            }

            if (_occlusionSubsystem.TryAcquireEnvironmentDepthCpuImage(out XRCpuImage depthBuffer))
            {
                depthTimestampMs = (ulong)(depthBuffer.timestamp * 1000);
                depthBuffer.Dispose();
            }

            return depthTimestampMs;
        }

        public float GetInstantDepthFPS()
        {
            if (_usingDepth && _canAutomaticallyTrackFPS)
            {
                return _instantDepthFPS;
            }

            return 0;
        }

        public ulong GetLatestSceneSegmentationTimestamp()
        {
            ulong sceneSegmentationTimestampMs = 0;

            if (!_usingSceneSegmentation)
            {
                return sceneSegmentationTimestampMs;
            }

            // If we have already acquired the latest frame, return the last timestamp
            if (_latestSceneSegmentationFrameId == _sceneSegmentationSubsystem.LatestFrameId)
            {
                return _lastTimeSceneSegmentation;
            }

            if (_sceneSegmentationSubsystem.TryAcquirePackedSceneSegmentationChannelsCpuImage(out XRCpuImage sceneSegmentationBuffer, out Matrix4x4 _))
            {
                sceneSegmentationTimestampMs = (ulong)(sceneSegmentationBuffer.timestamp * 1000);
                _latestSceneSegmentationFrameId = _sceneSegmentationSubsystem.LatestFrameId;
                sceneSegmentationBuffer.Dispose();
            }

            return sceneSegmentationTimestampMs;
        }

        public float GetInstantSceneSegmentationFPS()
        {
            if (_usingSceneSegmentation && _canAutomaticallyTrackFPS)
            {
                return _instantSceneSegmentationFPS;
            }

            return 0;
        }

        public ulong GetLatestMeshTimestamp()
        {
            if (!_usingMesh)
            {
                return 0;
            }

            return NsdkMeshingProvider.GetLastMeshUpdateTime();
        }

        public float GetInstantMeshFPS()
        {
            if (_usingMesh && _canAutomaticallyTrackFPS)
            {
                return _instantMeshFPS;
            }

            return 0;
        }

    }
}
