// Copyright 2022-2026 Niantic Spatial.

using System;
using System.Runtime.InteropServices;
using NianticSpatial.NSDK.AR.Core;

namespace NianticSpatial.NSDK.AR.Utilities.Logging
{
    internal interface IApi
    {
        void Log(LogLevel level, string log, string fileName, int fileLine, string funcname);
        void ChangeStdoutLoggerLogLevel(IntPtr unityContext, LogLevel level);
        void ChangeFileLoggerLogLevel(IntPtr unityContext, LogLevel level);
        void ChangeUnityLoggerLogLevel(IntPtr unityContext, LogLevel level);
    }

    internal class NativeApi : IApi
    {
        public void Log(LogLevel level, string log, string fileName, int fileLine, string funcname)
        {
            Lightship_ARDK_Unity_Log(level, log, fileName, fileLine, funcname);
        }

        public void ChangeStdoutLoggerLogLevel(IntPtr unityContext, LogLevel level)
        {
            Lightship_ARDK_Unity_Set_Stdout_Log_Level(unityContext, level);
        }

        public void ChangeFileLoggerLogLevel(IntPtr unityContext, LogLevel level)
        {
            Lightship_ARDK_Unity_Set_File_Log_Level(unityContext, level);
        }

        public void ChangeUnityLoggerLogLevel(IntPtr unityContext, LogLevel level)
        {
            Lightship_ARDK_Unity_Set_Callback_Log_Level(unityContext, level);
        }

        [DllImport(NsdkPlugin.Name)]
        private static extern void Lightship_ARDK_Unity_Log(LogLevel level, string log, string fileName, int fileLine, string funcname);

        [DllImport(NsdkPlugin.Name)]
        private static extern void Lightship_ARDK_Unity_Set_Stdout_Log_Level(IntPtr unityContext, LogLevel level);

        [DllImport(NsdkPlugin.Name)]
        private static extern void Lightship_ARDK_Unity_Set_File_Log_Level(IntPtr unityContext, LogLevel level);

        [DllImport(NsdkPlugin.Name)]
        private static extern void Lightship_ARDK_Unity_Set_Callback_Log_Level(IntPtr unityContext, LogLevel level);

    }
}
