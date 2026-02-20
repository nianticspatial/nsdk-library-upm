// Copyright Niantic Spatial.

using UnityEngine;

namespace NianticSpatial.NSDK.AR.XRSubsystems
{
    /// <summary>
    /// Structure describing device location in local AR coordinate space as
    /// calculated by VPS2.
    /// </summary>
    public struct XRVps2Pose
    {
        /// Device location in local AR coordinate space.
        public Pose Pose;

        /// Horizontal accuracy radius of the pose in meters.
        public float HorizontalAccuracy;

        /// Vertical accuracy radius of the pose in meters.
        public float VerticalAccuracy;

        /// Accuracy of yaw rotation in degrees.
        public float HeadingAccuracy;
    }
}
