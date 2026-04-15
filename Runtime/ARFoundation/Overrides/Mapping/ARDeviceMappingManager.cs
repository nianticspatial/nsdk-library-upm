// Copyright 2022-2026 Niantic Spatial.

using System;
using System.Collections;
using NianticSpatial.NSDK.AR.Common;
using NianticSpatial.NSDK.AR.Subsystems.DeviceMapping;
using NianticSpatial.NSDK.AR.Utilities;
using NianticSpatial.NSDK.AR.Utilities.Logging;
using NianticSpatial.NSDK.AR.XRSubsystems;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

namespace NianticSpatial.NSDK.AR.Mapping
{
    /// <summary>
    /// ARDeviceMappingManager can be used to create and manage maps on the local device.
    /// </summary>
    [Experimental]
    [PublicAPI]
    [DefaultExecutionOrder(NsdkARUpdateOrder.DeviceMappingManager)]
    public class ARDeviceMappingManager
        : SubsystemLifecycleManager<XRDeviceMappingSubsystem, XRDeviceMappingSubsystemDescriptor, XRDeviceMappingSubsystem.Provider>
    {
        /// <summary>
        /// Property access for mapping speed
        /// @note This is an experimental feature, and is subject to breaking changes or deprecation without notice
        /// </summary>
        [Experimental]
        public uint MappingTargetFrameRate
        {
            get => _mappingTargetFrameRate;
            set
            {
                _mappingTargetFrameRate = value;
                if (subsystem != null)
                {
                    subsystem.TargetFrameRate = value;
                }
            }
        }

        /// <summary>
        /// Property access for map splitting criteria by distance
        /// @note This is an experimental feature, and is subject to breaking changes or deprecation without notice
        /// </summary>
        [Experimental]
        public float MappingSplitterMaxDistanceMeters
        {
            get => _mappingSplitterMaxDistanceMeters;
            set
            {
                _mappingSplitterMaxDistanceMeters = value;
                if (subsystem != null)
                {
                    subsystem.SplitterMaxDistanceMeters = value;
                }
            }
        }

        /// <summary>
        /// Property access for map splitting criteria by time
        /// @note This is an experimental feature, and is subject to breaking changes or deprecation without notice
        /// </summary>
        [Experimental]
        public float MappingSplitterMaxDurationSeconds
        {
            get => _mappingSplitterMaxDurationSeconds;
            set
            {
                _mappingSplitterMaxDurationSeconds = value;
                if (subsystem != null)
                {
                    subsystem.SplitterMaxDurationSeconds = value;
                }
            }
        }

        /// <summary>
        /// A state if mapping is in progress or not. True is mapping is ongoing. Becomes false after calling StopMapping()
        /// @note This is an experimental feature, and is subject to breaking changes or deprecation without notice
        /// </summary>
        [Experimental]
        public bool IsMappingInProgress
        {
            get => subsystem != null && subsystem.IsMapping;
        }

        /// <summary>
        /// Get the root anchor payload if one has been created (as base64-encoded string).
        /// @note This is an experimental feature, and is subject to breaking changes or deprecation without notice
        /// </summary>
        [Experimental]
        public string RootAnchorPayload
        {
            get => _rootAnchorPayload;
        }

        /// <summary>
        /// Define how fast to run device mapping. Default is 0, meaning process every frame.
        /// @note This is an experimental feature, and is subject to breaking changes or deprecation without notice
        /// </summary>
        [SerializeField]
        [Experimental]
        private uint _mappingTargetFrameRate = 0;

        /// <summary>
        /// Define device map split based on how far the user traveled.
        /// @note This is an experimental feature, and is subject to breaking changes or deprecation without notice
        /// </summary>
        [SerializeField]
        [Experimental]
        private float _mappingSplitterMaxDistanceMeters = 10f;

        /// <summary>
        /// Define device map split based on how long in time the user mapped.
        /// @note This is an experimental feature, and is subject to breaking changes or deprecation without notice
        /// </summary>
        [SerializeField]
        [Experimental]
        private float _mappingSplitterMaxDurationSeconds = 10f;

        /// <summary>
        /// An event when device map data has been updated. The surfaced map contains only the parts of the map that
        /// were updated since the last time this event was invoked. Use TryGetMapData to get the entire map.
        /// @note This is an experimental feature, and is subject to breaking changes or deprecation without notice
        /// </summary>
        [Experimental]
        public event Action<byte[]> MapUpdated;

        /// <summary>
        /// An event when device map is finalized and ready to save
        /// @note This is an experimental feature, and is subject to breaking changes or deprecation without notice
        /// </summary>
        [Experimental]
        public event Action<byte[]> MapFinalized;

        private DeviceMapAccessController _deviceMapAccessController;

        private string _rootAnchorPayload; // Base64-encoded anchor payload string
        private byte[] _lastMapUpdate; // Store the last map update for finalized event

        private const float TimeoutToForceInvokeMapFinalizedEvent = 2.0f;

        private enum MappingState
        {
            Uninitialized,
            Mapping,
            Stopped,
        }
        private MappingState _state = MappingState.Uninitialized;

        private void Awake()
        {
            _deviceMapAccessController = DeviceMapAccessController.Acquire();
        }

        protected override void OnBeforeStart()
        {
            _state = MappingState.Uninitialized;
            subsystem.TargetFrameRate = _mappingTargetFrameRate;
            subsystem.SplitterMaxDistanceMeters = _mappingSplitterMaxDistanceMeters;
            subsystem.SplitterMaxDurationSeconds = _mappingSplitterMaxDurationSeconds;
        }

        protected override void OnDisable()
        {
            _state = MappingState.Uninitialized;
            base.OnDisable();
        }

        protected override void OnDestroy()
        {
            _deviceMapAccessController?.Release();
            _deviceMapAccessController = null;
        }

        private void Update()
        {
            if (subsystem == null)
            {
                return;
            }

            // Check map updates every 10 frames
            // TODO: make it configurable how often processing map sync
            if (Time.frameCount % 10 != 0)
            {
                return;
            }

            // collect map updates
            TryUpdateMap();
        }

        /// <summary>
        ///  Start map generation
        /// </summary>
        public void StartMapping()
        {
            if (subsystem == null)
            {
                return;
            }

            _state = MappingState.Mapping;
            subsystem.StartMapping();
        }

        /// <summary>
        /// Stop map generation
        /// </summary>
        public void StopMapping()
        {
            if (subsystem == null)
            {
                return;
            }

            subsystem.StopMapping();
            StartCoroutine(MonitorFinalizedEventCoroutine());
            _state = MappingState.Stopped;
        }

        public void ClearMap()
        {
            if (_deviceMapAccessController == null)
            {
                return;
            }

            _rootAnchorPayload = null;
            _deviceMapAccessController.ClearDeviceMaps();
        }

        public bool TryGetMapData(out byte[] data)
        {
            if (_deviceMapAccessController == null)
            {
                data = null;
                return false;
            }

            return _deviceMapAccessController.GetMapData(out data);
        }

        /// <summary>
        /// Extract the metadata from a map relative to the stored root anchor.
        /// @note This is an experimental feature, and is subject to breaking changes or deprecation without notice
        /// </summary>
        /// <param name="mapData">The map to extract the metadata from.</param>
        /// <param name="points">The positions of the feature points in the map.</param>
        /// <param name="errors">The error metric for each of the points in the map.</param>
        /// <returns>True if successful, false otherwise.</returns>
        public bool ExtractMapMetadataFromRootAnchor(byte[] mapData, out Vector3[] points, out float[] errors)
        {
            points = null;
            errors = null;

            if (_deviceMapAccessController == null)
            {
                return false;
            }

            if (string.IsNullOrEmpty(_rootAnchorPayload))
            {
                Log.Error("Root anchor has not been created. Keep scanning and wait for a map to be created.");
                return false;
            }

            if (mapData == null || mapData.Length == 0)
            {
                Log.Error("Map data is empty");
                return false;
            }

            return _deviceMapAccessController.ExtractMapMetadataFromAnchor(
                _rootAnchorPayload,
                mapData,
                out points,
                out errors);
        }

        private void TryUpdateMap()
        {
            if (_deviceMapAccessController == null)
            {
                return;
            }

            // Get the latest map update
            bool gotNewData = _deviceMapAccessController.GetMapUpdate(out var mapUpdateData);

            if (!gotNewData || mapUpdateData == null || mapUpdateData.Length == 0)
            {
                return;
            }

            // Check if we have a root anchor, if not create one
            if (string.IsNullOrEmpty(_rootAnchorPayload))
            {
                var anchorCreated = _deviceMapAccessController.CreateRootAnchor(out _rootAnchorPayload);
                if (!anchorCreated || string.IsNullOrEmpty(_rootAnchorPayload))
                {
                    return;
                }
            }

            // Extract metadata to check if there are actual new points and errors
            var metadataExtracted = ExtractMapMetadataFromRootAnchor(mapUpdateData, out Vector3[] points, out _);

            // Only store and emit update if metadata extraction succeeded and has meaningful data
            if (metadataExtracted && points != null && points.Length > 0)
            {
                // Store the latest map update
                _lastMapUpdate = mapUpdateData;

                // Invoke update event with the newest map update
                MapUpdated?.Invoke(mapUpdateData);
            }

            // Invoke finalized event if stopped and received (last) map update
            if (_state == MappingState.Stopped)
            {
                MapFinalized?.Invoke(mapUpdateData);
                _state = MappingState.Uninitialized;
            }
        }

        private IEnumerator MonitorFinalizedEventCoroutine()
        {
            // TODO: modify native side to tell final map update or not, instead of this way
            yield return new WaitForSeconds(TimeoutToForceInvokeMapFinalizedEvent);

            if (_state == MappingState.Stopped && _lastMapUpdate != null)
            {
                MapFinalized?.Invoke(_lastMapUpdate);
                _state = MappingState.Uninitialized;
            }
        }
    }
}
