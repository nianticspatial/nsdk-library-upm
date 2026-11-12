// Copyright Niantic Spatial.

using UnityEngine;

namespace NianticSpatial.NSDK.AR.XRSubsystems
{
    /// <summary>
    /// Pose in AR session space calculated by VPS2 from a geolocation. This is the pose of
    /// the geolocation that was passed in, not of the device.
    /// </summary>
    public struct XRVps2Pose
    {
        /// The converted pose, in AR session space: not composed with the XR origin. Position is
        /// in meters from the AR session space origin.
        public Pose Pose;

        /// Horizontal accuracy radius of the pose in meters.
        public float HorizontalAccuracy;

        /// Vertical accuracy radius of the pose in meters.
        public float VerticalAccuracy;

        /// Accuracy of yaw rotation in degrees.
        public float HeadingAccuracy;

        /// Tracking state of the localization this pose was computed from. Always
        /// Coarse or Precise when a pose is returned (never Unavailable).
        public Vps2TrackingState TrackingState;
    }
}
