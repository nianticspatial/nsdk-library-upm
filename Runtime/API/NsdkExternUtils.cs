// Copyright 2022-2026 Niantic.

using System;
using System.Runtime.InteropServices;
using NianticSpatial.NSDK.AR.Core;

namespace NianticSpatial.NSDK.AR.API
{
    internal static class NsdkExternUtils
    {
        public static void ReleaseResource(IntPtr handle)
        {
            NativeApi.ARDK_Release_Resource(handle);
        }

        private static class NativeApi
        {
            [DllImport(NsdkPlugin.Name, EntryPoint = "ARDK_Release_Resource")]
            public static extern void ARDK_Release_Resource(IntPtr resource);
        }
    }
}
