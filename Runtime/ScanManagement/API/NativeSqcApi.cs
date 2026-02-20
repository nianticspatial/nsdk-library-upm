// Copyright 2022-2026 Niantic Spatial.

using System;
using System.Runtime.InteropServices;
using NianticSpatial.NSDK.AR.Utilities.Logging;
using NianticSpatial.NSDK.AR.Core;
using UnityEngine;

namespace NianticSpatial.NSDK.AR.Scanning
{
    internal class NativeSqcApi : ISqcApi
    {
        public IntPtr SQCCreate(IntPtr unityContext)
        {
            return Native.Lightship_ARDK_Unity_Scanning_SQC_Create(unityContext);
        }

        public void SQCRelease(IntPtr handle)
        {
            Native.Lightship_ARDK_Unity_Scanning_SQC_Release(handle);
        }

        public bool SQCRun(IntPtr handle, float framerate, string scanPath)
        {
            return Native.Lightship_ARDK_Unity_Scanning_SQC_Run(handle, framerate, scanPath);
        }

        public void SQCCancelCurrentRun(IntPtr handle)
        {
            Native.Lightship_ARDK_Unity_Scanning_SQC_Interrupt(handle);
        }

        public bool SQCIsRunning(IntPtr handle)
        {
            return Native.Lightship_ARDK_Unity_Scanning_SQC_Is_Running(handle);
        }

        public float SQCGetProgress(IntPtr handle)
        {
            return Native.Lightship_ARDK_Unity_Scanning_SQC_Get_Progress(handle);
        }

        public void SQCGetResult(IntPtr handle, string scanPath, IntPtr scores, out int scoresSize)
        {
            Native.Lightship_ARDK_Unity_Scanning_SQC_Get_Result(handle, scanPath, scores, out UInt32 size);
            scoresSize = (int)(size);

            if (scoresSize < 0)
                Log.Info($"Integer overflow error? Value of variable \"{nameof(scoresSize)}\" is {scoresSize}");
        }

        private static class Native
        {
            [DllImport(NsdkPlugin.Name)]
            public static extern IntPtr Lightship_ARDK_Unity_Scanning_SQC_Create(IntPtr unityContext);

            [DllImport(NsdkPlugin.Name)]
            public static extern void Lightship_ARDK_Unity_Scanning_SQC_Release(IntPtr handle);

            [DllImport(NsdkPlugin.Name)]
            public static extern bool Lightship_ARDK_Unity_Scanning_SQC_Run(IntPtr handle, float framerate,
                string scanPath);

            [DllImport(NsdkPlugin.Name)]
            public static extern void Lightship_ARDK_Unity_Scanning_SQC_Interrupt(IntPtr handle);

            [DllImport(NsdkPlugin.Name)]
            public static extern bool Lightship_ARDK_Unity_Scanning_SQC_Is_Running(IntPtr handle);

            [DllImport(NsdkPlugin.Name)]
            public static extern float Lightship_ARDK_Unity_Scanning_SQC_Get_Progress(IntPtr handle);

            [DllImport(NsdkPlugin.Name)]
            public static extern void Lightship_ARDK_Unity_Scanning_SQC_Get_Result
            (
                IntPtr handle,
                string scanPath,
                IntPtr outScores,
                out UInt32 outSize
            );
        }
    }
}
