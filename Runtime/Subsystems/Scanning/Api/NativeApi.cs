// Copyright 2022-2026 Niantic Spatial.

using System;
using System.Runtime.InteropServices;
using System.Text;
using NianticSpatial.NSDK.AR.Core;
using NianticSpatial.NSDK.AR.Utilities.Logging;
using UnityEngine;

namespace NianticSpatial.NSDK.AR.Subsystems.Scanning
{
    internal class NativeApi : IApi
    {
        public IntPtr Construct(IntPtr unityContext)
        {
            return Native.Create(unityContext);
        }

        public void Destruct(IntPtr handle)
        {
            Native.Release(handle);
        }

        public void Start(IntPtr handle)
        {
            Native.Start(handle);
        }

        public void Stop(IntPtr handle)
        {
            Native.Stop(handle);
        }

        public void Configure
        (
            IntPtr handle,
            ScannerConfigurationCStruct config
        )
        {
            Native.Configure
            (
                handle,
                config
            );
        }

        public IntPtr TryGetRaycastBuffer
        (
            IntPtr handle,
            out IntPtr colorBuffer,
            out IntPtr normalBuffer,
            out IntPtr positionBuffer,
            out int colorSize,
            out int normalSize,
            out int positionSize,
            out int width,
            out int height
        )
        {
            colorSize = 0;
            width = 0;
            height = 0;
            normalSize = 0;
            positionSize = 0;

            return
                Native.TryGetRaycastBuffer
                (
                    handle,
                    out colorBuffer,
                    out normalBuffer,
                    out positionBuffer,
                    out colorSize,
                    out normalSize,
                    out positionSize,
                    out width,
                    out height
                );
        }

        public void SaveCurrentScan(IntPtr handle)
        {
            Native.SaveCurrentScan(handle);
        }

        private StringBuilder _scanIdBuffer = new StringBuilder(128);

        public int GetFrameCount(IntPtr handle)
        {
            return Native.GetFrameCount(handle);
        }

        public bool TryGetRecordingInfo(IntPtr handle, out string scanId, out RecordingStatus status)
        {
            _scanIdBuffer.Clear();
            if (handle == IntPtr.Zero ||
                !Native.GetRecordingInfo(handle, _scanIdBuffer, _scanIdBuffer.Capacity, out status) ||
                _scanIdBuffer.Length == 0)
            {
                scanId = null;
                status = RecordingStatus.Unknown;
                return false;
            }
            scanId = _scanIdBuffer.ToString();
            return true;
        }

        public IntPtr TryGetVoxelBuffer
        (
            IntPtr handle,
            out IntPtr positionBuffer,
            out IntPtr colorBuffer,
            out IntPtr normalBuffer,
            out int pointCount
        )
        {
            return Native.TryGetVoxelBuffer(handle, out positionBuffer, out colorBuffer, out normalBuffer, out pointCount);
        }

        public void ComputeVoxels(IntPtr handle)
        {
            Native.ComputeVoxels(handle);
        }

        public float GetVoxelSize(IntPtr handle)
        {
            return Native.GetVoxelSize(handle);
        }

        public void ReleaseResource(IntPtr handle, IntPtr resourceHandle)
        {
            Native.ReleaseResource(handle, resourceHandle);
        }

        private static class Native
        {
            [DllImport(NsdkPlugin.Name, EntryPoint = "Lightship_ARDK_Unity_Scanner_Create")]
            public static extern IntPtr Create(IntPtr unityContext);

            [DllImport(NsdkPlugin.Name, EntryPoint = "Lightship_ARDK_Unity_Scanner_Release")]
            public static extern void Release(IntPtr handle);

            [DllImport(NsdkPlugin.Name, EntryPoint = "Lightship_ARDK_Unity_Scanner_Start")]
            public static extern void Start(IntPtr handle);

            [DllImport(NsdkPlugin.Name, EntryPoint = "Lightship_ARDK_Unity_Scanner_Stop")]
            public static extern void Stop(IntPtr handle);

            [DllImport(NsdkPlugin.Name, EntryPoint = "Lightship_ARDK_Unity_Scanner_Get_Raycast_Buffer")]
            public static extern IntPtr TryGetRaycastBuffer
            (
                IntPtr handle,
                out IntPtr colorBuffer,
                out IntPtr normalBuffer,
                out IntPtr positionBuffer,
                out int colorSize,
                out int normalSize,
                out int positionSize,
                out int width,
                out int height
            );

            [DllImport(NsdkPlugin.Name, EntryPoint = "Lightship_ARDK_Unity_Scanner_Save_Current_Scan")]
            public static extern void SaveCurrentScan(IntPtr handle);

            [DllImport(NsdkPlugin.Name, EntryPoint = "Lightship_ARDK_Unity_Scanner_Get_Recording_Info")]
            public static extern bool GetRecordingInfo
                (IntPtr handle, StringBuilder scanId, int maxScanIdLen, out RecordingStatus status);

            [DllImport(NsdkPlugin.Name, EntryPoint = "Lightship_ARDK_Unity_Scanner_Release_Resource")]
            public static extern void ReleaseResource(IntPtr handle, IntPtr resourceHandle);

            [DllImport(NsdkPlugin.Name, EntryPoint = "Lightship_ARDK_Unity_Scanner_Compute_Voxels")]
            public static extern void ComputeVoxels(IntPtr handle);

            
            [DllImport(NsdkPlugin.Name, EntryPoint = "Lightship_ARDK_Unity_Scanner_Get_Frame_Count")]
            public static extern int GetFrameCount(IntPtr handle);

            [DllImport(NsdkPlugin.Name, EntryPoint = "Lightship_ARDK_Unity_Scanner_Get_Voxel_Buffer")]
            public static extern IntPtr TryGetVoxelBuffer
                (IntPtr handle, out IntPtr positionBuffer, out IntPtr colorBuffer, out IntPtr normalBuffer, out int pointCount);

            [DllImport(NsdkPlugin.Name, EntryPoint = "Lightship_ARDK_Unity_Scanner_Get_Voxel_Size")]
            public static extern float GetVoxelSize(IntPtr handle);

            [DllImport(NsdkPlugin.Name, EntryPoint = "Lightship_ARDK_Unity_Scanner_Configure")]
            public static extern void Configure(IntPtr handle, ScannerConfigurationCStruct config);
        }
    }
}
