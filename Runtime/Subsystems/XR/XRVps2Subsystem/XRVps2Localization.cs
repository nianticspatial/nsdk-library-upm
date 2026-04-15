// Copyright Niantic Spatial.

using System;
using Unity.Mathematics;

namespace NianticSpatial.NSDK.AR.XRSubsystems
{
    /// <summary>
    /// Spatial mapping between the device’s AR coordinate space and real-world geolocation,
    /// as determined by VPS2.
    /// </summary>
    /// <note>
    /// The localization is a point-in-time snapshot. Updates to VPS2’s estimate are not reflected in
    /// previously retrieved localization instances.
    /// </note>
    public struct XRVps2Localization: IEquatable<XRVps2Localization>
    {
        /// <summary>
        /// The state of VPS2 tracking. The other fields in this struct are only valid if
        /// the tracking state is not <see cref="Vps2TrackingState.Unavailable"/>.
        /// </summary>
        public Vps2TrackingState TrackingState;

        /// <summary>Estimated latitude of the camera in degrees.</summary>
        internal double ReferenceLatitude;

        /// <summary>Estimated longitude of the camera in degrees.</summary>
        internal double ReferenceLongitude;

        /// <summary>Estimated altitude of the camera in metres.</summary>
        internal double ReferenceAltitude;

        /// <summary>
        /// Transform from tracking to lat/lon/alt in metres relative to reference point.
        /// </summary>
        internal double4x4 TrackingToEdn;

        public bool Equals(XRVps2Localization other) {
            return
                TrackingState == other.TrackingState
                && ReferenceLatitude.Equals(other.ReferenceLatitude)
                && ReferenceLongitude.Equals(other.ReferenceLongitude)
                && ReferenceAltitude.Equals(other.ReferenceAltitude)
                && TrackingToEdn.Equals(other.TrackingToEdn);
        }

        public override bool Equals(object obj) => obj is XRVps2Localization other && Equals(other);

        public override int GetHashCode() {
            return HashCode.Combine(
                (int)TrackingState,
                ReferenceLatitude,
                ReferenceLongitude,
                ReferenceAltitude,
                TrackingToEdn.GetHashCode()
            );
        }
    }
}
