// Copyright 2022-2026 Niantic Spatial.

using System;
using System.Runtime.InteropServices;

namespace NianticSpatial.NSDK.AR.API
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct NsdkMatrix4F
    {
        // In C#, we'll represent this as a pointer that can be marshaled
        public IntPtr Values;
    }
}
