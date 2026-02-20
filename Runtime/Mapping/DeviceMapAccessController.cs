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
    /// Class to access primitive device map data and configs using the new ARDK_MapStorage_* API
    /// @note This is an experimental feature, and is subject to breaking changes or deprecation without notice
    /// </summary>
    [Experimental]
    [PublicAPI]
    public class DeviceMapAccessController
    {
        private IntPtr _unityContextHandleCache = IntPtr.Zero;
        private static DeviceMapAccessController _instance;

        private static object _instanceLock = new object();
        public static DeviceMapAccessController Instance
        {
            get
            {
                lock (_instanceLock)
                {
                    // If NsdkUnityContext is not initialized, return null
                    if (NsdkUnityContext.UnityContextHandle == IntPtr.Zero)
                    {
                        if (_instance != null)
                        {
                            _instance.Destroy();
                            _instance = null;
                        }
                        return null;
                    }

                    // If the instance hasn't been created yet, create it and initialize it
                    if (_instance == null)
                    {
                        _instance = new DeviceMapAccessController();
                        // Deregister no-ops if we haven't registered yet. Prevents double registration
                        //  in case someone else destroys the instance
                        NsdkUnityContext.OnDeinitialized -= DestroyNativeInstance;
                        NsdkUnityContext.OnDeinitialized += DestroyNativeInstance;

                        _instance.Init();
                        return _instance;
                    }

                    // If the Unity context handle hasn't changed, return the instance
                    if (_instance._unityContextHandleCache == NsdkUnityContext.UnityContextHandle)
                    {
                        Debug.Log("Returning cached instance");
                        return _instance;
                    }

                    // If the Unity context handle has changed, destroy the native instance and create a new one
                    _instance.Destroy();
                    _instance.Init();

                    return _instance;
                }
            }
        }

        internal DeviceMapAccessController()
        {
            _api = new NativeMapStorageAccessApi();
        }

        private IMapStorageAccessApi _api;

        internal void Init()
        {
            if (NsdkUnityContext.UnityContextHandle == IntPtr.Zero)
            {
                Log.Error("Unity context handle is not initialized yet. " +
                    "DeviceMapAccessController cannot be initialized");
                return;
            }

            if (_unityContextHandleCache == NsdkUnityContext.UnityContextHandle)
            {
                Log.Warning("DeviceMapAccessController is already initialized.");
                return;
            }

            _api = new NativeMapStorageAccessApi();
            _unityContextHandleCache = NsdkUnityContext.UnityContextHandle;
            
            if (!_api.Create(_unityContextHandleCache))
            {
                Log.Error("Failed to create map storage access API");
            }
        }

        internal void Destroy()
        {
            if (_api == null)
            {
                return;
            }

            _api.Destroy();
            _api.Dispose();
            _unityContextHandleCache = IntPtr.Zero;
            _api = null;
        }

        /// <summary>
        /// Clear map storage.
        /// @note This is an experimental feature, and is subject to breaking changes or deprecation without notice
        /// </summary>
        [Experimental]
        public bool ClearDeviceMap()
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
        /// <param name="anchorPayload">The payload of the anchor, encoded as a base64 string.</param>
        /// <returns>True if successful, false otherwise.</returns>
        [Experimental]
        public bool CreateRootAnchor(out byte[] anchorPayload)
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
            byte[] anchorPayload,
            byte[] mapData,
            out Vector3[] points,
            out float[] errors,
            out bool usesLearnedFeatures)
        {
            points = null;
            errors = null;
            usesLearnedFeatures = false;

            if (anchorPayload == null || anchorPayload.Length == 0)
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

            return _api.ExtractMapMetadataFromAnchor(anchorPayload, mapData, out points, out errors, out usesLearnedFeatures);
        }

        internal void UseFakeMapStorageAccessApi(IMapStorageAccessApi mapStorageAccessApi)
        {
            _api = mapStorageAccessApi;
        }

        private static void DestroyNativeInstance()
        {
            lock (_instanceLock)
            {
                _instance?.Destroy();
            }
        }
    }
}
