// Copyright Niantic Spatial.

namespace NianticSpatial.NSDK.AR.XRSubsystems
{
    /// <summary>
    /// Structure describing device location and heading as calculated by VPS2.
    /// </summary>
    public struct XRVps2Geolocation
    {
        /// Device location and heading.
        public XRGeolocation Geolocation;

        /// Horizontal accuracy radius of the location in meters.
        public float HorizontalAccuracy;

        /// Vertical accuracy radius of the location in meters.
        public float VerticalAccuracy;

        /// Accuracy of heading reading in degrees.
        public float HeadingAccuracy;
    }
}
