// Copyright Niantic Spatial.

using UnityEngine;

namespace NianticSpatial.NSDK.AR.XRSubsystems
{
    /// <summary>
    /// Structure describing device location and heading.
    /// </summary>
    public struct XRGeolocation
    {
        /// Geographical device location latitude.
        public double Latitude;

        /// Geographical device location longitude.
        public double Longitude;

        /// Geographical device location altitude in meters.
        public double Altitude;

        /// The heading in degrees relative to the geographic North Pole.
        public double HeadingEdn;

        /// Device orientation in the East-Down-North (EDN) frame.
        public Quaternion OrientationEdn;
    }
}
