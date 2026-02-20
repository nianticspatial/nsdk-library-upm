// Copyright 2022-2026 Niantic Spatial.

using System;
using System.Runtime.InteropServices;
using NianticSpatial.NSDK.AR.Core;
using NianticSpatial.NSDK.AR.Utilities;
using NianticSpatial.NSDK.AR.Utilities.Logging;
using UnityEngine;

namespace NianticSpatial.NSDK.AR.Mapping.Api
{
    internal class NativeMapStorageAccessApi : IMapStorageAccessApi
    {
        private IntPtr _unityContextHandle;
        private bool _isDisposed;

        public bool Create(IntPtr unityContextHandle)
        {
            if (!NsdkUnityContext.CheckUnityContext(unityContextHandle))
            {
                Log.Error("NativeMapStorageAccessApi: Invalid Unity context handle");
                return false;
            }

            _unityContextHandle = unityContextHandle;
            int status = Lightship_ARDK_Unity_MapStorageAccess_Create(unityContextHandle);
            return status == 0; // ARDK_Status_OK = 0
        }

        public bool Destroy()
        {
            if (_isDisposed || !_unityContextHandle.IsValidHandle())
            {
                return false;
            }

            int status = Lightship_ARDK_Unity_MapStorageAccess_Destroy(_unityContextHandle);
            return status == 0; // ARDK_Status_OK = 0
        }

        public void Dispose()
        {
            if (_isDisposed)
            {
                return;
            }

            Destroy();
            _unityContextHandle = IntPtr.Zero;
            _isDisposed = true;
        }

        public bool GetMapData(out byte[] mapData)
        {
            mapData = null;

            if (!CheckUnityContext())
            {
                return false;
            }

            IntPtr dataPtr = IntPtr.Zero;
            uint dataSize = 0;
            IntPtr resourceHandle = IntPtr.Zero;

            int status = Lightship_ARDK_Unity_MapStorageAccess_GetMapData(
                _unityContextHandle,
                out dataPtr,
                out dataSize,
                out resourceHandle);

            if (status != 0 || dataPtr == IntPtr.Zero || dataSize == 0)
            {
                if (resourceHandle.IsValidHandle())
                {
                    Lightship_ARDK_Unity_MapStorageAccess_ReleaseResource(resourceHandle);
                }
                return false;
            }

            try
            {
                mapData = new byte[dataSize];
                Marshal.Copy(dataPtr, mapData, 0, (int)dataSize);
                return true;
            }
            finally
            {
                if (resourceHandle.IsValidHandle())
                {
                    Lightship_ARDK_Unity_MapStorageAccess_ReleaseResource(resourceHandle);
                }
            }
        }

        public bool GetMapUpdate(out byte[] mapUpdateData)
        {
            mapUpdateData = null;

            if (!CheckUnityContext())
            {
                return false;
            }

            IntPtr dataPtr = IntPtr.Zero;
            uint dataSize = 0;
            IntPtr resourceHandle = IntPtr.Zero;

            int status = Lightship_ARDK_Unity_MapStorageAccess_GetMapUpdate(
                _unityContextHandle,
                out dataPtr,
                out dataSize,
                out resourceHandle);

            if (status != 0 || dataPtr == IntPtr.Zero || dataSize == 0)
            {
                if (resourceHandle.IsValidHandle())
                {
                    Lightship_ARDK_Unity_MapStorageAccess_ReleaseResource(resourceHandle);
                }
                return false;
            }

            try
            {
                mapUpdateData = new byte[dataSize];
                Marshal.Copy(dataPtr, mapUpdateData, 0, (int)dataSize);
                return true;
            }
            finally
            {
                if (resourceHandle.IsValidHandle())
                {
                    Lightship_ARDK_Unity_MapStorageAccess_ReleaseResource(resourceHandle);
                }
            }
        }

        public bool AddMap(byte[] mapData)
        {
            if (!CheckUnityContext() || mapData == null || mapData.Length == 0)
            {
                return false;
            }

            IntPtr dataPtr = IntPtr.Zero;
            unsafe
            {
                fixed (byte* bytePtr = mapData)
                {
                    dataPtr = (IntPtr)bytePtr;
                    int status = Lightship_ARDK_Unity_MapStorageAccess_AddMap(
                        _unityContextHandle,
                        dataPtr,
                        (uint)mapData.Length);
                    return status == 0; // ARDK_Status_OK = 0
                }
            }
        }

        public bool Clear()
        {
            if (!CheckUnityContext())
            {
                return false;
            }

            int status = Lightship_ARDK_Unity_MapStorageAccess_Clear(_unityContextHandle);
            return status == 0; // ARDK_Status_OK = 0
        }

        public bool MergeMapUpdate(byte[] existingMapData, byte[] mapUpdateData, out byte[] mergedMapData)
        {
            mergedMapData = null;

            if (existingMapData == null || existingMapData.Length == 0 ||
                mapUpdateData == null || mapUpdateData.Length == 0)
            {
                return false;
            }

            IntPtr mergedDataPtr = IntPtr.Zero;
            uint mergedDataSize = 0;
            IntPtr resourceHandle = IntPtr.Zero;

            IntPtr existingDataPtr = IntPtr.Zero;
            IntPtr updateDataPtr = IntPtr.Zero;

            unsafe
            {
                fixed (byte* existingPtr = existingMapData)
                {
                    existingDataPtr = (IntPtr)existingPtr;
                    fixed (byte* updatePtr = mapUpdateData)
                    {
                        updateDataPtr = (IntPtr)updatePtr;

                        int status = Lightship_ARDK_Unity_MapStorageAccess_MergeMapUpdate(
                            existingDataPtr,
                            (uint)existingMapData.Length,
                            updateDataPtr,
                            (uint)mapUpdateData.Length,
                            out mergedDataPtr,
                            out mergedDataSize,
                            out resourceHandle);

                        if (status != 0 || mergedDataPtr == IntPtr.Zero || mergedDataSize == 0)
                        {
                            if (resourceHandle.IsValidHandle())
                            {
                                Lightship_ARDK_Unity_MapStorageAccess_ReleaseResource(resourceHandle);
                            }
                            return false;
                        }

                        try
                        {
                            mergedMapData = new byte[mergedDataSize];
                            Marshal.Copy(mergedDataPtr, mergedMapData, 0, (int)mergedDataSize);
                            return true;
                        }
                        finally
                        {
                            if (resourceHandle.IsValidHandle())
                            {
                                Lightship_ARDK_Unity_MapStorageAccess_ReleaseResource(resourceHandle);
                            }
                        }
                    }
                }
            }
        }

        public bool CreateRootAnchor(out byte[] anchorPayload)
        {
            anchorPayload = null;

            if (!CheckUnityContext())
            {
                return false;
            }

            IntPtr payloadPtr = IntPtr.Zero;
            uint payloadSize = 0;
            IntPtr resourceHandle = IntPtr.Zero;

            int status = Lightship_ARDK_Unity_MapStorageAccess_CreateRootAnchor(
                _unityContextHandle,
                out payloadPtr,
                out payloadSize,
                out resourceHandle);

            if (status != 0 || payloadPtr == IntPtr.Zero || payloadSize == 0)
            {
                if (resourceHandle.IsValidHandle())
                {
                    Lightship_ARDK_Unity_MapStorageAccess_ReleaseResource(resourceHandle);
                }
                return false;
            }

            try
            {
                anchorPayload = new byte[payloadSize];
                Marshal.Copy(payloadPtr, anchorPayload, 0, (int)payloadSize);
                return true;
            }
            finally
            {
                if (resourceHandle.IsValidHandle())
                {
                    Lightship_ARDK_Unity_MapStorageAccess_ReleaseResource(resourceHandle);
                }
            }
        }

        public bool ExtractMapMetadataFromAnchor(byte[] anchorPayload, byte[] mapData, out Vector3[] points, out float[] errors, out bool usesLearnedFeatures)
        {
            points = null;
            errors = null;
            usesLearnedFeatures = false;

            if (!CheckUnityContext() || anchorPayload == null || anchorPayload.Length == 0 || mapData == null || mapData.Length == 0)
            {
                return false;
            }

            IntPtr pointsPtr = IntPtr.Zero;
            IntPtr errorsPtr = IntPtr.Zero;
            uint pointsCount = 0;
            bool usesLearnedFeaturesValue = false;
            IntPtr resourceHandle = IntPtr.Zero;

            IntPtr mapDataPtr = IntPtr.Zero;
            unsafe
            {
                fixed (byte* mapPtr = mapData)
                {
                    mapDataPtr = (IntPtr)mapPtr;
                    fixed (byte* anchorPtr = anchorPayload)
                    {
                        int status = Lightship_ARDK_Unity_MapStorageAccess_ExtractMapMetadataFromAnchor(
                            _unityContextHandle,
                            (IntPtr)anchorPtr,
                            (uint)anchorPayload.Length,
                            mapDataPtr,
                            (uint)mapData.Length,
                            out pointsPtr,
                            out errorsPtr,
                            out pointsCount,
                            out usesLearnedFeaturesValue,
                            out resourceHandle);

                        if (status != 0 || pointsPtr == IntPtr.Zero || errorsPtr == IntPtr.Zero || pointsCount == 0)
                        {
                            if (resourceHandle.IsValidHandle())
                            {
                                Lightship_ARDK_Unity_MapStorageAccess_ReleaseResource(resourceHandle);
                            }
                            return false;
                        }

                        try
                        {
                            // Copy points data (x, y, z for each point)
                            float[] pointsArray = new float[pointsCount * 3];
                            Marshal.Copy(pointsPtr, pointsArray, 0, pointsArray.Length);

                            // Copy errors data
                            errors = new float[pointsCount];
                            Marshal.Copy(errorsPtr, errors, 0, (int)pointsCount);

                            // Convert points array to Vector3 array
                            points = new Vector3[pointsCount];
                            for (int i = 0; i < pointsCount; i++)
                            {
                                points[i] = new Vector3(
                                    pointsArray[i * 3],
                                    -pointsArray[i * 3 + 1],
                                    pointsArray[i * 3 + 2]);
                            }

                            usesLearnedFeatures = usesLearnedFeaturesValue;
                            return true;
                        }
                        finally
                        {
                            if (resourceHandle.IsValidHandle())
                            {
                                Lightship_ARDK_Unity_MapStorageAccess_ReleaseResource(resourceHandle);
                            }
                        }
                    }
                }
            }
        }

        private bool CheckUnityContext()
        {
            if (_isDisposed || !_unityContextHandle.IsValidHandle())
            {
                Log.Warning("NativeMapStorageAccessApi: No valid Unity context handle");
                return false;
            }

            return true;
        }

        // Native function declarations

        [DllImport(NsdkPlugin.Name)]
        private static extern int Lightship_ARDK_Unity_MapStorageAccess_Create(IntPtr unity_context_handle);

        [DllImport(NsdkPlugin.Name)]
        private static extern int Lightship_ARDK_Unity_MapStorageAccess_Destroy(IntPtr unity_context_handle);

        [DllImport(NsdkPlugin.Name)]
        private static extern int Lightship_ARDK_Unity_MapStorageAccess_GetMapData(
            IntPtr unity_context_handle,
            out IntPtr map_data_out,
            out uint map_data_size_out,
            out IntPtr resource_handle_out);

        [DllImport(NsdkPlugin.Name)]
        private static extern int Lightship_ARDK_Unity_MapStorageAccess_GetMapUpdate(
            IntPtr unity_context_handle,
            out IntPtr map_update_data_out,
            out uint map_update_data_size_out,
            out IntPtr resource_handle_out);

        [DllImport(NsdkPlugin.Name)]
        private static extern int Lightship_ARDK_Unity_MapStorageAccess_AddMap(
            IntPtr unity_context_handle,
            IntPtr map_data,
            uint map_data_size);

        [DllImport(NsdkPlugin.Name)]
        private static extern int Lightship_ARDK_Unity_MapStorageAccess_Clear(IntPtr unity_context_handle);

        [DllImport(NsdkPlugin.Name)]
        private static extern int Lightship_ARDK_Unity_MapStorageAccess_MergeMapUpdate(
            IntPtr existing_map_data,
            uint existing_map_data_size,
            IntPtr map_update_data,
            uint map_update_data_size,
            out IntPtr merged_map_data_out,
            out uint merged_map_data_size_out,
            out IntPtr resource_handle_out);

        [DllImport(NsdkPlugin.Name)]
        private static extern int Lightship_ARDK_Unity_MapStorageAccess_CreateRootAnchor(
            IntPtr unity_context_handle,
            out IntPtr payload_out,
            out uint payload_size_out,
            out IntPtr resource_handle_out);

        [DllImport(NsdkPlugin.Name)]
        private static extern int Lightship_ARDK_Unity_MapStorageAccess_ExtractMapMetadataFromAnchor(
            IntPtr unity_context_handle,
            IntPtr anchor_payload_data,
            uint anchor_payload_size,
            IntPtr map_data,
            uint map_data_size,
            out IntPtr points_out,
            out IntPtr errors_out,
            out uint points_count_out,
            [MarshalAs(UnmanagedType.I1)] out bool uses_learned_features_out,
            out IntPtr resource_handle_out);

        [DllImport(NsdkPlugin.Name)]
        private static extern void Lightship_ARDK_Unity_MapStorageAccess_ReleaseResource(IntPtr resource_handle);
    }
}
