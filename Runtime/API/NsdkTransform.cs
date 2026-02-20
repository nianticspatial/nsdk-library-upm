// Copyright Niantic Spatial.

using System.Runtime.InteropServices;

namespace NianticSpatial.NSDK.AR.API
{
    // Defined in ardk_transform.h
    [StructLayout(LayoutKind.Sequential)]
    internal struct NsdkTransform
    {
        public float translation_x;
        public float translation_y;
        public float translation_z;
        public float scale_xyz;
        public float orientation_x;
        public float orientation_y;
        public float orientation_z;
        public float orientation_w;
    }
}
