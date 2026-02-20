// Copyright 2022-2026 Niantic Spatial.

using System;
using System.Runtime.InteropServices;

namespace NianticSpatial.NSDK.AR.API
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct NsdkBuffer
    {
        public IntPtr data; // const uint8_t*
        public UInt32 data_size;
    }
}
