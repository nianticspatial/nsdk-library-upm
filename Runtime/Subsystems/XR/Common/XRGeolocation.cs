// Copyright Niantic Spatial.

using UnityEngine;

namespace NianticSpatial.NSDK.AR.XRSubsystems
{
    /// <summary>
    /// Structure describing a location and heading. Coordinates use the WGS84 datum.
    /// </summary>
    /// <remarks>
    /// Depending on the API, this describes the device, a VPS asset's origin, or an anchor.
    /// </remarks>
    public struct XRGeolocation
    {
        /// WGS84 latitude, in degrees.
        public double Latitude;

        /// WGS84 longitude, in degrees.
        public double Longitude;

        /// Altitude in meters above the WGS84 ellipsoid (not mean sea level).
        public double Altitude;

        /// The heading in degrees relative to the geographic North Pole.
        public double Heading;

        /// Device orientation in the East-Down-North (EDN) frame.
        public Quaternion OrientationEdn;
    }
}
