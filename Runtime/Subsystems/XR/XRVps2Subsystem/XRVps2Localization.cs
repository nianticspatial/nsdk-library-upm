// Copyright Niantic Spatial.

using System;
using Unity.Mathematics;

namespace NianticSpatial.NSDK.AR.XRSubsystems
{
    /// <summary>
    /// Spatial mapping between the device’s AR tracking space and real-world geolocation,
    /// as determined by VPS2.
    /// </summary>
    /// <remarks>
    /// This relates two origins: the AR session space origin and a geographic reference point.
    /// It is not relative to the current camera pose, and does not change as the device moves;
    /// it changes only when VPS2 refines or replaces its localization. To convert a specific
    /// pose or geolocation, pass it to <see cref="XRVps2Subsystem.GetPose(XRVps2Localization, double, double, double, UnityEngine.Quaternion)"/>
    /// or <see cref="XRVps2Subsystem.TryGetGeolocation"/>. All distances are in meters.
    /// </remarks>
    /// <note>
    /// The localization is a point-in-time snapshot. Updates to VPS2’s estimate are not reflected in
    /// previously retrieved localization instances.
    /// </note>
    public struct XRVps2Localization: IEquatable<XRVps2Localization>
    {
        /// <summary>
        /// The state of VPS2 tracking. The geolocation fields in this struct are only valid if
        /// the tracking state is not <see cref="Vps2TrackingState.Unavailable"/>.
        /// </summary>
        public Vps2TrackingState TrackingState;

        /// <summary>
        /// The VPS asset the device is localized to, independent of the tracking state.
        /// Null until a visual localization succeeds against a submitted asset. Place the
        /// asset's content with <see cref="XRVps2Subsystem.TryGetAssetTrackingData"/>.
        /// </summary>
        public Vps2LocalizedAsset? LocalizedAsset;

        /// <summary>
        /// WGS84 latitude of the reference point, in degrees. The reference point is not the
        /// camera: it is the georeference origin of the localized map (Precise), or a fixed
        /// origin chosen by the coarse localizer (Coarse). Only meaningful together with
        /// <see cref="TrackingToEdn"/> from the same snapshot.
        /// </summary>
        internal double ReferenceLatitude;

        /// <summary>WGS84 longitude of the reference point, in degrees.</summary>
        internal double ReferenceLongitude;

        /// <summary>
        /// Altitude of the reference point, in meters above the WGS84 ellipsoid.
        /// </summary>
        internal double ReferenceAltitude;

        /// <summary>
        /// Transform from AR tracking space to a local East-Down-North frame centered on the
        /// reference point, in meters. Kept in the native NSDK convention (OpenCV tracking axes),
        /// not Unity axes. Independent of the current camera pose.
        /// </summary>
        internal double4x4 TrackingToEdn;

        public bool Equals(XRVps2Localization other) {
            return
                TrackingState == other.TrackingState
                && LocalizedAsset.Equals(other.LocalizedAsset)
                && ReferenceLatitude.Equals(other.ReferenceLatitude)
                && ReferenceLongitude.Equals(other.ReferenceLongitude)
                && ReferenceAltitude.Equals(other.ReferenceAltitude)
                && TrackingToEdn.Equals(other.TrackingToEdn);
        }

        public override bool Equals(object obj) => obj is XRVps2Localization other && Equals(other);

        public override int GetHashCode() {
            return HashCode.Combine(
                (int)TrackingState,
                LocalizedAsset,
                ReferenceLatitude,
                ReferenceLongitude,
                ReferenceAltitude,
                TrackingToEdn.GetHashCode()
            );
        }
    }
}
