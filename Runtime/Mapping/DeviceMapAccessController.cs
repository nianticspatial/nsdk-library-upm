// Copyright 2022-2026 Niantic Spatial.

using System;
using NianticSpatial.NSDK.AR.Core;
using NianticSpatial.NSDK.AR.Mapping.Api;
using NianticSpatial.NSDK.AR.Utilities;
using NianticSpatial.NSDK.AR.Utilities.Logging;
using UnityEngine;

namespace NianticSpatial.NSDK.AR.Mapping
{
    /// <summary>
    /// Class to access primitive device map data and configs.
    /// </summary>
    [PublicAPI]
    public class DeviceMapAccessController
    {
        private IntPtr _unityContextHandle;
        private IMapStorageAccessApi _api;
        private bool _disposed;

        private static DeviceMapAccessController _instance;
        private static readonly object _instanceLock = new object();

        /// <summary>
        /// Gets or creates the shared map access object.
        /// </summary>
        /// <returns>The shared DeviceMapAccessController instance, or null if one could not be acquired.</returns>
        [Experimental]
        public static DeviceMapAccessController Acquire()
        {
            lock (_instanceLock)
            {
                var unityContextHandle = NsdkUnityContext.UnityContextHandle;
                if (unityContextHandle == IntPtr.Zero)
                {
                    _instance?.Release();
                    _instance = null;
                    return null;
                }

                if (_instance != null && _instance._unityContextHandle == unityContextHandle)
                {
                    return _instance;
                }

                // Context handle changed or no instance yet — recreate
                _instance?.Release();
                _instance = new DeviceMapAccessController(unityContextHandle);

                // Clean up when the native context is torn down
                NsdkUnityContext.OnDeinitialized -= ReleaseInstance;
                NsdkUnityContext.OnDeinitialized += ReleaseInstance;

                return _instance;
            }
        }

        private static void ReleaseInstance()
        {
            lock (_instanceLock)
            {
                _instance?.Release();
                _instance = null;
            }
        }

        private DeviceMapAccessController(IntPtr unityContextHandle)
        {
            _unityContextHandle = unityContextHandle;
            _api = new NativeMapStorageAccessApi();

            if (!_api.Acquire(_unityContextHandle))
            {
                Log.Error("Failed to create map storage access ");
            }
        }

        public void Release()
        {
            if (_api != null)
            {
                _api.Release();
                _api = null;
            }

            _unityContextHandle = IntPtr.Zero;
        }

        /// <summary>
        /// Clear map storage.
        /// @note This is an experimental feature, and is subject to breaking changes or deprecation without notice
        /// </summary>
        [Experimental]
        public bool ClearDeviceMaps()
        {
            if (_api == null)
            {
                return false;
            }

            return _api.Clear();
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

            if (_api == null)
            {
                return false;
            }

            return _api.GetMapData(out mapData);
        }

        /// <summary>
        /// Get the latest map update data, which consists of the new nodes and edges
        /// that have been added since the last call to this function.
        /// @note This is an experimental feature, and is subject to breaking changes or deprecation without notice
        /// </summary>
        /// <param name="mapUpdateData">The serialized map update data will be written here.</param>
        /// <returns>True if successful, false otherwise.</returns>
        [Experimental]
        public bool GetMapUpdate(out byte[] mapUpdateData)
        {
            mapUpdateData = null;

            if (_api == null)
            {
                return false;
            }

            return _api.GetMapUpdate(out mapUpdateData);
        }

        /// <summary>
        /// Add serialized map data to the map storage.
        /// @note This is an experimental feature, and is subject to breaking changes or deprecation without notice
        /// </summary>
        /// <param name="mapData">The serialized map data to add.</param>
        /// <returns>True if successful, false otherwise.</returns>
        [Experimental]
        public bool AddMap(byte[] mapData)
        {
            if (mapData == null || mapData.Length == 0)
            {
                Log.Error("Map data is empty");
                return false;
            }

            if (_api == null)
            {
                return false;
            }

            // Try to decode as ARDK3 device map and swap if converted successfully
            var didConvert = ARDeviceMapCompatibilityUtil.TryConvertArdk3ToNsdk(mapData, out var nsdkDeviceMap);
            if (didConvert)
            {
                // swap the blob data with converted map blob data
                mapData = nsdkDeviceMap;
            }

            return _api.AddMap(mapData);
        }

        /// <summary>
        /// Merge a map update into an existing map.
        /// @note This is an experimental feature, and is subject to breaking changes or deprecation without notice
        /// </summary>
        /// <param name="existingMapData">The existing map to merge the update into.</param>
        /// <param name="mapUpdateData">The map update to merge into the existing map.</param>
        /// <param name="mergedMapData">The merged map will be written here.</param>
        /// <returns>True if successful, false otherwise.</returns>
        [Experimental]
        public bool MergeMapUpdate(byte[] existingMapData, byte[] mapUpdateData, out byte[] mergedMapData)
        {
            mergedMapData = null;

            if (existingMapData == null || existingMapData.Length == 0)
            {
                Log.Error("Existing map data is empty");
                return false;
            }

            if (mapUpdateData == null || mapUpdateData.Length == 0)
            {
                Log.Error("Map update data is empty");
                return false;
            }

            if (_api == null)
            {
                return false;
            }

            return _api.MergeMapUpdate(existingMapData, mapUpdateData, out mergedMapData);
        }

        /// <summary>
        /// Creates an anchor on the existing map located at the origin of the current
        /// AR session, if possible.
        /// @note This is an experimental feature, and is subject to breaking changes or deprecation without notice
        /// </summary>
        /// <param name="anchorPayload">The payload of the anchor, as a base64-encoded string.</param>
        /// <returns>True if successful, false otherwise.</returns>
        [Experimental]
        public bool CreateRootAnchor(out string anchorPayload)
        {
            anchorPayload = null;

            if (_api == null)
            {
                return false;
            }

            return _api.CreateRootAnchor(out anchorPayload);
        }

        /// <summary>
        /// Extract the metadata from a map relative to an anchor on the map.
        /// @note This is an experimental feature, and is subject to breaking changes or deprecation without notice
        /// </summary>
        /// <param name="anchorPayload">The anchor payload as a byte array. The returned map metadata
        /// will be relative to this anchor.</param>
        /// <param name="mapData">The map to extract the metadata from.</param>
        /// <param name="points">The positions of the feature points in the map.</param>
        /// <param name="errors">The error metric for each of the points in the map.</param>
        /// <param name="usesLearnedFeatures">Whether the map uses learned features.</param>
        /// <returns>True if successful, false otherwise.</returns>
        [Experimental]
        public bool ExtractMapMetadataFromAnchor(
            string anchorPayload,
            byte[] mapData,
            out Vector3[] points,
            out float[] errors)
        {
            points = null;
            errors = null;

            if (string.IsNullOrEmpty(anchorPayload))
            {
                Log.Error("Anchor payload is empty");
                return false;
            }

            if (mapData == null || mapData.Length == 0)
            {
                Log.Error("Map data is empty");
                return false;
            }

            if (_api == null)
            {
                return false;
            }

            // Convert base64 string back to byte array for the API call
            byte[] anchorPayloadBytes = System.Text.Encoding.UTF8.GetBytes(anchorPayload);
            return _api.ExtractMapMetadataFromAnchor(anchorPayloadBytes, mapData, out points, out errors);
        }

        internal void UseFakeMapStorageAccessApi(IMapStorageAccessApi mapStorageAccessApi)
        {
            _api = mapStorageAccessApi;
        }
    }
}
