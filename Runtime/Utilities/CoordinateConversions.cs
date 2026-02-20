// Copyright 2022-2026 Niantic Spatial.

using UnityEngine;

namespace NianticSpatial.NSDK.AR.Utilities
{
    /// Utility methods to convert between NSDK and Unity coordinate frames
    internal static class CoordinateConversions
    {
        private static readonly Vector3 s_signedYCoordinateVector = new Vector3(1, -1, 1);

        private static readonly Vector3 s_signedZCoordinateVector = new Vector3(1, 1, -1);

        private static readonly Matrix4x4 s_signedYCordinateMatrix4x4 =
            new Matrix4x4
            (
                new Vector4(1, 0, 0, 0),
                new Vector4(0, -1, 0, 0),
                new Vector4(0, 0, 1, 0),
                new Vector4(0, 0, 0, 1)
            );

        private static readonly Matrix4x4 s_signedZCoordinateMatrix4x4 =
            new Matrix4x4
            (
                new Vector4(1, 0, 0, 0),
                new Vector4(0, 1, 0, 0),
                new Vector4(0, 0, -1, 0),
                new Vector4(0, 0, 0, 1)
            );


        public static Vector3 FromOpenGLToUnity(this Vector3 point)
        {
            return Vector3.Scale(point, s_signedZCoordinateVector);
        }

        public static Vector3 FromUnityToOpenGL(this Vector3 point)
        {
            return point.FromOpenGLToUnity();
        }

        public static Vector3 FromNsdkToUnity(this Vector3 point)
        {
            return Vector3.Scale(point, s_signedYCoordinateVector);
        }

        public static Vector3 FromUnityToNsdk(this Vector3 point)
        {
            // Conversion is just switching signs, so the same vector scaling
            // works both ways
            return point.FromNsdkToUnity();
        }

        public static Matrix4x4 FromNsdkToUnity(this Matrix4x4 matrix)
        {
            // Sy [R|T] Sy
            //    [0|1]
            return s_signedYCordinateMatrix4x4 * matrix * s_signedYCordinateMatrix4x4;
        }

        public static Matrix4x4 FromUnityToNsdk(this Matrix4x4 matrix)
        {
            return matrix.FromNsdkToUnity();
        }

        public static Matrix4x4 FromOpenGLToUnity(this Matrix4x4 matrix)
        {
            // Sz [R|T] Sz
            //    [0|1]
            return s_signedZCoordinateMatrix4x4 * matrix * s_signedZCoordinateMatrix4x4;
        }

        public static Matrix4x4 FromUnityToOpenGL(this Matrix4x4 matrix)
        {
            return matrix.FromOpenGLToUnity();
        }
    }
}
