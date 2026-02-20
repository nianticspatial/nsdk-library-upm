// Copyright 2022-2026 Niantic Spatial.

using System;
using System.Runtime.InteropServices;
using NianticSpatial.NSDK.AR.Core;
using AOT;

namespace NianticSpatial.NSDK.AR.Utilities.DeterministicPlayback
{
    internal sealed class ComputeMonitor : IDisposable
    {
        private IntPtr _nativeHandle;

        public ComputeMonitor(IntPtr unityContext)
        {
            var coreContext = NsdkUnityContext.GetCoreContext(unityContext);
            _nativeHandle = Native.ARDK_ComputeMonitorHandle_Acquire(coreContext);
            PrepareToCompute();
        }

        ~ComputeMonitor()
        {
            Dispose();
        }

        public void Dispose()
        {
            if (!_nativeHandle.IsValidHandle()) return;
            Native.ARDK_ComputeMonitorHandle_Release(_nativeHandle);
            _nativeHandle = IntPtr.Zero;
            GC.SuppressFinalize(this);
        }

        public void PrepareToCompute()
        {
            Native.ARDK_ComputeMonitor_PrepareToCompute(_nativeHandle);
        }
        public bool IsDoneComputing()
        {
            return Native.ARDK_ComputeMonitor_IsDoneComputing(_nativeHandle);
        }

        private static class Native
        {
            [DllImport(NsdkPlugin.Name)]
            public static extern IntPtr ARDK_ComputeMonitorHandle_Acquire(
                IntPtr coreContextHandle);

            [DllImport(NsdkPlugin.Name)]
            public static extern void ARDK_ComputeMonitorHandle_Release(IntPtr computeMonitorHandle);

            [DllImport(NsdkPlugin.Name)]
            public static extern bool ARDK_ComputeMonitor_PrepareToCompute(IntPtr computeMonitorHandle);

            [DllImport(NsdkPlugin.Name)]
            public static extern bool ARDK_ComputeMonitor_IsDoneComputing(IntPtr computeMonitorHandle);


        }
    }
}
