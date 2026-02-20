// Copyright Niantic Spatial.

using NianticSpatial.NSDK.AR.API;
using UnityEngine;

namespace NianticSpatial.NSDK.AR.Utilities
{
    internal static class PoseExtensions
    {
        public static Pose FromNsdkToUnity(this NsdkTransform transform)
        {
            var position =
                new Vector3
                (
                    transform.translation_x,
                    transform.translation_y,
                    transform.translation_z
                );

            var rotation =
                new Quaternion
                (
                    transform.orientation_x,
                    transform.orientation_y,
                    transform.orientation_z,
                    transform.orientation_w
                );

            var unity = Matrix4x4.TRS(position, rotation, Vector3.one).FromNsdkToUnity();
            return new Pose(unity.GetPosition(), unity.rotation);
        }

        public static NsdkTransform FromUnityToNsdk(this Pose pose)
        {
            var ardk = Matrix4x4.TRS(pose.position, pose.rotation, Vector3.one).FromUnityToNsdk();
            var position = ardk.GetPosition();
            var rotation = ardk.rotation;
            var transform = new NsdkTransform();
            transform.translation_x = position.x;
            transform.translation_y = position.y;
            transform.translation_z = position.z;
            transform.scale_xyz = 1f;
            transform.orientation_x = rotation.x;
            transform.orientation_y = rotation.y;
            transform.orientation_z = rotation.z;
            transform.orientation_w = rotation.w;
            return transform;
        }
    }
}
