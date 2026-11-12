// Copyright 2022-2026 Niantic Spatial.

using System;
using System.Runtime.InteropServices;
using NianticSpatial.NSDK.AR.API;
using NianticSpatial.NSDK.AR.Core;

namespace NianticSpatial.NSDK.AR.Subsystems.Dashcam
{
    // Mirrors the native ARDK_Dashcam_Config struct.
    [StructLayout(LayoutKind.Sequential)]
    internal struct NsdkDashcamConfig
    {
        public int Framerate;
        public int MaxBufferSeconds;
        public int MaxBufferMemoryMb;
        public NsdkString BasePath;
    }

    // Mirrors the native ARDK_Dashcam_BufferInfo struct.
    [StructLayout(LayoutKind.Sequential)]
    internal struct NsdkDashcamBufferInfo
    {
        public uint FrameCount;
        public ulong MemoryUsedBytes;
        public ulong DurationMs;
    }

    internal enum NsdkDashcamSaveState
    {
        NotAvailable = 0,
        Saved = 1,
        Failed = 2,
    }

    // Mirrors the native ARDK_Dashcam_SaveResult struct.
    [StructLayout(LayoutKind.Sequential)]
    internal struct NsdkDashcamSaveResult
    {
        public IntPtr Handle;
        public NsdkDashcamSaveState State;
        public IntPtr SavePath;
        public uint SavePathLen;
        public IntPtr ArchivePath;
        public uint ArchivePathLen;
    }

    /// <summary>
    /// Default native-backed implementation of <see cref="IApi"/> using P/Invoke.
    /// </summary>
    internal class NativeApi : IApi
    {
        public NsdkStatus Create(IntPtr nsdkHandle) => (NsdkStatus)Native.Create(nsdkHandle);

        public NsdkStatus Destroy(IntPtr nsdkHandle) => (NsdkStatus)Native.Destroy(nsdkHandle);

        public NsdkStatus Configure(IntPtr nsdkHandle, ref NsdkDashcamConfig config) =>
            (NsdkStatus)Native.Configure(nsdkHandle, ref config);

        public NsdkStatus Start(IntPtr nsdkHandle) => (NsdkStatus)Native.Start(nsdkHandle);

        public NsdkStatus Stop(IntPtr nsdkHandle) => (NsdkStatus)Native.Stop(nsdkHandle);

        public NsdkStatus GetBufferInfo(IntPtr nsdkHandle, out NsdkDashcamBufferInfo info) =>
            (NsdkStatus)Native.GetBufferInfo(nsdkHandle, out info);

        public NsdkStatus TriggerSave(IntPtr nsdkHandle) => (NsdkStatus)Native.TriggerSave(nsdkHandle);

        public NsdkStatus GetSaveResult(IntPtr nsdkHandle, out NsdkDashcamSaveResult result) =>
            (NsdkStatus)Native.GetSaveResult(nsdkHandle, out result);

        public int GetFeatureStatus(IntPtr nsdkHandle) => Native.GetFeatureStatus(nsdkHandle);

        private static class Native
        {
            [DllImport(NsdkPlugin.Name, EntryPoint = "ARDK_Dashcam_Create")]
            public static extern int Create(IntPtr nsdkHandle);

            [DllImport(NsdkPlugin.Name, EntryPoint = "ARDK_Dashcam_Destroy")]
            public static extern int Destroy(IntPtr nsdkHandle);

            [DllImport(NsdkPlugin.Name, EntryPoint = "ARDK_Dashcam_Configure")]
            public static extern int Configure(IntPtr nsdkHandle, ref NsdkDashcamConfig config);

            [DllImport(NsdkPlugin.Name, EntryPoint = "ARDK_Dashcam_Start")]
            public static extern int Start(IntPtr nsdkHandle);

            [DllImport(NsdkPlugin.Name, EntryPoint = "ARDK_Dashcam_Stop")]
            public static extern int Stop(IntPtr nsdkHandle);

            [DllImport(NsdkPlugin.Name, EntryPoint = "ARDK_Dashcam_GetBufferInfo")]
            public static extern int GetBufferInfo(IntPtr nsdkHandle, out NsdkDashcamBufferInfo info);

            [DllImport(NsdkPlugin.Name, EntryPoint = "ARDK_Dashcam_TriggerSave")]
            public static extern int TriggerSave(IntPtr nsdkHandle);

            [DllImport(NsdkPlugin.Name, EntryPoint = "ARDK_Dashcam_GetSaveResult")]
            public static extern int GetSaveResult(IntPtr nsdkHandle, out NsdkDashcamSaveResult result);

            [DllImport(NsdkPlugin.Name, EntryPoint = "ARDK_Dashcam_GetFeatureStatus")]
            public static extern int GetFeatureStatus(IntPtr nsdkHandle);
        }
    }
}
