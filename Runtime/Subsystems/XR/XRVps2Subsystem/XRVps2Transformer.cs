// Copyright Niantic Spatial.

using System;
using Unity.Mathematics;

namespace NianticSpatial.NSDK.AR.XRSubsystems
{
    /// <summary>
    /// Struct containing the metadata required to perform bidirectional conversions between the application's kicak
    /// AR tracking space and the global coordinate system (lat/lng/alt/heading).
    /// </summary>
    /// <note>
    /// The transformer is a point-in-time snapshot. Updates to VPS2’s estimate are not reflected in
    /// previously retrieved transformer instances.
    /// </note>
    public struct XRVps2Transformer: IEquatable<XRVps2Transformer>
    {
        public Vps2TrackingState TrackingState;
        internal double ReferenceLatitude;
        internal double ReferenceLongitude;
        internal double ReferenceAltitude;
        internal double4x4 TrackingToEdn;

        public bool Equals(XRVps2Transformer other) {
            return 
                TrackingState == other.TrackingState
                && ReferenceLatitude.Equals(other.ReferenceLatitude)
                && ReferenceLongitude.Equals(other.ReferenceLongitude)
                && ReferenceAltitude.Equals(other.ReferenceAltitude)
                && TrackingToEdn.Equals(other.TrackingToEdn);
        }

        public override bool Equals(object obj) => obj is XRVps2Transformer other && Equals(other);

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
