// Copyright 2022-2026 Niantic Spatial.

using System;
using System.Collections;
using System.Collections.Generic;
using NianticSpatial.NSDK.AR.Utilities;
using NianticSpatial.NSDK.AR.Utilities.Logging;
using UnityEngine;

namespace NianticSpatial.NSDK.AR.Mapping
{
    /// <summary>
    /// ARDeviceMappingManager can be used to generate device map using the new ARDK_MapStorage_* API
    /// @note This is an experimental feature, and is subject to breaking changes or deprecation without notice
    /// </summary>
    [Experimental]
    [PublicAPI]
    public class ARDeviceMappingManager : MonoBehaviour
    {
        // Public properties

        /// <summary>
        /// Get DeviceMapAccessController, which provides primitive access to the device map and related info
        /// @note This is an experimental feature, and is subject to breaking changes or deprecation without notice
        /// </summary>
        [Experimental]
        public DeviceMapAccessController DeviceMapAccessController
        {
            get;
            private set;
        }

        /// <summary>
        /// Get DeviceMappingController, which provides primitive API for device mapping
        /// @note This is an experimental feature, and is subject to breaking changes or deprecation without notice
        /// </summary>
        [Experimental]
        public DeviceMappingController DeviceMappingController
        {
            get;
        } = new();

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
            }
        }

        /// <summary>
        /// A state if mapping is in progress or not. True is mapping is ongoing. Becomes false after calling StopMapping()
        /// @note This is an experimental feature, and is subject to breaking changes or deprecation without notice
        /// </summary>
        [Experimental]
        public bool IsMappingInProgress
        {
            get => DeviceMappingController.IsMapping;
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
        private uint _mappingTargetFrameRate = DeviceMappingController.DefaultTargetFrameRate;

        /// <summary>
        /// Define device map split based on how far the user traveled.
        /// @note This is an experimental feature, and is subject to breaking changes or deprecation without notice
        /// </summary>
        [SerializeField]
        [Experimental]
        private float _mappingSplitterMaxDistanceMeters = DeviceMappingController.DefaultSplitterMaxDistanceMeters;

        /// <summary>
        /// Define device map split based on how long in time the user mapped.
        /// @note This is an experimental feature, and is subject to breaking changes or deprecation without notice
        /// </summary>
        [SerializeField]
        [Experimental]
        private float _mappingSplitterMaxDurationSeconds = DeviceMappingController.DefaultSplitterMaxDurationSeconds;

        // Events

        /// <summary>
        /// An event when device map data has been updated
        /// @note This is an experimental feature, and is subject to breaking changes or deprecation without notice
        /// </summary>
        [Experimental]
        public event Action<byte[]> DeviceMapUpdated;

        /// <summary>
        /// An event when device map is finalized and ready to save
        /// @note This is an experimental feature, and is subject to breaking changes or deprecation without notice
        /// </summary>
        [Experimental]
        public event Action<byte[]> DeviceMapFinalized;

        // private vars

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

        // Monobehaviour methods
        private void Awake()
        {
            DeviceMapAccessController = DeviceMapAccessController.Instance;
            DeviceMappingController.Init();
        }

        private void OnEnable()
        {
            DeviceMappingController.UpdateConfiguration();
            DeviceMappingController.StartNativeModule();
        }

        private void OnDisable()
        {
            _state = MappingState.Uninitialized;
            DeviceMappingController.StopNativeModule();
        }

        private void Start()
        {
            _state = MappingState.Uninitialized;
            DeviceMappingController.TargetFrameRate = _mappingTargetFrameRate;
            DeviceMappingController.SplitterMaxDistanceMeters = _mappingSplitterMaxDistanceMeters;
            DeviceMappingController.SplitterMaxDurationSeconds = _mappingSplitterMaxDurationSeconds;
        }

        private void OnDestroy()
        {
            DeviceMapAccessController?.Destroy();
            DeviceMappingController.Destroy();
        }

        private void Update()
        {
            // Check map updates every 10 frames
            // TODO: make it configurable how often processing map sync
            if (Time.frameCount % 10 != 0)
            {
                return;
            }

            // collect map updates
            TryUpdateMap();
        }

        // public methods

        /// <summary>
        /// Asynchronously restarts the underlying module with the current configuration.
        /// </summary>
        public IEnumerator RestartModuleAsyncCoroutine()
        {
            DeviceMappingController.TargetFrameRate = _mappingTargetFrameRate;
            DeviceMappingController.SplitterMaxDistanceMeters = _mappingSplitterMaxDistanceMeters;
            DeviceMappingController.SplitterMaxDurationSeconds = _mappingSplitterMaxDurationSeconds;
            DeviceMappingController.UpdateConfiguration();

            // Clear root anchor payload since restarting the module clears the map data store
            // A new root anchor will be created when mapping starts and map updates arrive
            _rootAnchorPayload = null;
            _lastMapUpdate = null;

            // restart native modules to enable new configs
            yield return null;
            DeviceMappingController.StopNativeModule();
            yield return null;
            DeviceMappingController.StartNativeModule();
        }

        /// <summary>
        ///  Start map generation
        /// @note This is an experimental feature, and is subject to breaking changes or deprecation without notice
        /// </summary>
        [Experimental]
        public void StartMapping()
        {
            _state = MappingState.Mapping;

            // Run mapping
            DeviceMappingController.StartMapping();
        }

        /// <summary>
        /// Stop map generation
        /// @note This is an experimental feature, and is subject to breaking changes or deprecation without notice
        /// </summary>
        [Experimental]
        public void StopMapping()
        {
            DeviceMappingController.StopMapping();
            StartCoroutine(MonitorFinalizedEventCoroutine());

            _state = MappingState.Stopped;
        }

        /// <summary>
        /// Get the current map data.
        /// @note This is an experimental feature, and is subject to breaking changes or deprecation without notice
        /// </summary>
        /// <param name="mapData">The serialized map data will be written here.</param>
        /// <returns>True if successful, false otherwise.</returns>
        [Experimental]
        public bool GetMapData(out byte[] mapData)
        {
            mapData = null;

            if (DeviceMapAccessController == null)
            {
                return false;
            }

            return DeviceMapAccessController.GetMapData(out mapData);
        }

        /// <summary>
        /// Creates an anchor on the existing map located at the origin of the current
        /// AR session, if possible. The anchor payload is stored and can be retrieved via RootAnchorPayload property.
        /// @note If using alongside ARPersistantAnchorManager, make sure this is called before, so that the mapping anchor
        /// can be seen by ARPersistantAnchorManager
        /// </summary>
        /// <returns>True if successful, false otherwise.</returns>
        [Experimental]
        public bool CreateRootAnchor()
        {
            if (DeviceMapAccessController == null)
            {
                return false;
            }

            bool success = DeviceMapAccessController.CreateRootAnchor(out var anchorPayload);
            if (success && anchorPayload != null && anchorPayload.Length > 0)
            {
                // Convert base64-encoded bytes to string
                _rootAnchorPayload = System.Text.Encoding.UTF8.GetString(anchorPayload);
            }

            return success;
        }

        /// <summary>
        /// Extract the metadata from a map relative to the stored root anchor.
        /// @note This is an experimental feature, and is subject to breaking changes or deprecation without notice
        /// </summary>
        /// <param name="mapData">The map to extract the metadata from.</param>
        /// <param name="points">The positions of the feature points in the map.</param>
        /// <param name="errors">The error metric for each of the points in the map.</param>
        /// <param name="usesLearnedFeatures">Whether the map uses learned features.</param>
        /// <returns>True if successful, false otherwise.</returns>
        [Experimental]
        public bool ExtractMapMetadataFromRootAnchor(
            byte[] mapData,
            out Vector3[] points,
            out float[] errors,
            out bool usesLearnedFeatures)
        {
            points = null;
            errors = null;
            usesLearnedFeatures = false;

            if (DeviceMapAccessController == null)
            {
                return false;
            }

            if (string.IsNullOrEmpty(_rootAnchorPayload))
            {
                Log.Error("Root anchor has not been created. Call CreateRootAnchor() first.");
                return false;
            }

            if (mapData == null || mapData.Length == 0)
            {
                Log.Error("Map data is empty");
                return false;
            }

            // Convert base64 string back to byte array for the API call
            byte[] anchorPayloadBytes = System.Text.Encoding.UTF8.GetBytes(_rootAnchorPayload);

            return DeviceMapAccessController.ExtractMapMetadataFromAnchor(
                anchorPayloadBytes,
                mapData,
                out points,
                out errors,
                out usesLearnedFeatures);
        }

        // Private methods

        private void TryUpdateMap()
        {
            if (DeviceMapAccessController == null)
            {
                return;
            }

            // Get the latest map update
            bool gotNewData = DeviceMapAccessController.GetMapUpdate(out var mapUpdateData);

            if (!gotNewData || mapUpdateData == null || mapUpdateData.Length == 0)
            {
                return;
            }

            // Check if we have a root anchor, if not create one
            if (string.IsNullOrEmpty(_rootAnchorPayload))
            {
                bool anchorCreated = false;
                try
                {
                    anchorCreated = CreateRootAnchor();
                }
                catch (Exception e)
                {
                    Log.Error($"Exception creating root anchor: {e}");
                }

                if (!anchorCreated || string.IsNullOrEmpty(_rootAnchorPayload))
                {
                    return;
                }
            }

            // Extract metadata to check if there are actual new points and errors
            bool metadataExtracted = ExtractMapMetadataFromRootAnchor(
                mapUpdateData,
                out Vector3[] points,
                out float[] errors,
                out bool usesLearnedFeatures);

            // Only store and emit update if metadata extraction succeeded and has meaningful data
            if (metadataExtracted && points != null && points.Length > 0)
            {
                // Store the latest map update
                _lastMapUpdate = mapUpdateData;

                // Invoke update event with the newest map update
                DeviceMapUpdated?.Invoke(mapUpdateData);
            }

            // Invoke finalized event if stopped and received (last) map update
            if (_state == MappingState.Stopped)
            {
                DeviceMapFinalized?.Invoke(mapUpdateData);
                _state = MappingState.Uninitialized;
            }
        }

        private IEnumerator MonitorFinalizedEventCoroutine()
        {
            // TODO: modify native side to tell final map update or not, instead of this way
            yield return new WaitForSeconds(TimeoutToForceInvokeMapFinalizedEvent);

            if (_state == MappingState.Stopped && _lastMapUpdate != null)
            {
                DeviceMapFinalized?.Invoke(_lastMapUpdate);
                _state = MappingState.Uninitialized;
            }
        }
    }
}
