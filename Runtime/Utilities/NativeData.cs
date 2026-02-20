// Copyright 2022-2026 Niantic Spatial.

using System;
using System.Runtime.InteropServices;

namespace NianticSpatial.NSDK.AR.Utilities
{
    /// <summary>
    /// This definition must precisely match the native layer's definition
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    internal struct NativeStringStruct
    {
        public IntPtr CharArrayIntPtr;
        public UInt32 ArrayLength;
    }
}
