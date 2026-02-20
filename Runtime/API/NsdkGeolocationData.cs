// Copyright Niantic Spatial.

using System.Runtime.InteropServices;

namespace NianticSpatial.NSDK.AR.API
{
    // Defined in ardk_geolocation_data.h
    [StructLayout(LayoutKind.Sequential)]
    internal struct NsdkGeolocationData
    {
        public double latitude;
        public double longitude;
        public double altitude;
        public double heading_edn;
        public float orientation_edn_x;
        public float orientation_edn_y;
        public float orientation_edn_z;
        public float orientation_edn_w;
    }
}
