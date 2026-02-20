// Copyright Niantic Spatial.

using Unity.Mathematics;
using UnityEngine;
using Matrix4x4 = UnityEngine.Matrix4x4;

namespace NianticSpatial.NSDK.AR.Utilities
{
    internal static class Matrix4x4Extensions
    {
        public static float[] ToRowMajorArray(this Matrix4x4 matrix)
        {
            return new[]
            {
                matrix.m00, matrix.m01, matrix.m02, matrix.m03, matrix.m10, matrix.m11, matrix.m12, matrix.m13,
                matrix.m20, matrix.m21, matrix.m22, matrix.m23, matrix.m30, matrix.m31, matrix.m32, matrix.m33,
            };
        }

        public static float[] ToColumnMajorArray(this Matrix4x4 matrix)
        {
            return new[]
            {
                matrix.m00, matrix.m10, matrix.m20, matrix.m30, matrix.m01, matrix.m11, matrix.m21, matrix.m31,
                matrix.m02, matrix.m12, matrix.m22, matrix.m32, matrix.m03, matrix.m13, matrix.m23, matrix.m33
            };
        }

        public static Matrix4x4 FromColumnMajorArray(this float[] array)
        {
            return new Matrix4x4
            (
                new Vector4(array[0], array[1], array[2], array[3]),
                new Vector4(array[4], array[5], array[6], array[7]),
                new Vector4(array[8], array[9], array[10], array[11]),
                new Vector4(array[12], array[13], array[14], array[15])
            );
        }

        public static double4x4 FromColumnMajorArray(this double[] array)
        {
            return new double4x4
            (
                new double4(array[0], array[1], array[2], array[3]),
                new double4(array[4], array[5], array[6], array[7]),
                new double4(array[8], array[9], array[10], array[11]),
                new double4(array[12], array[13], array[14], array[15])
            );
        }

        public static double[] ToColumnMajorArray(this double4x4 matrix)
        {
            return new []
            {
                matrix.c0.x, matrix.c0.y, matrix.c0.z, matrix.c0.w,
                matrix.c1.x, matrix.c1.y, matrix.c1.z, matrix.c1.w,
                matrix.c2.x, matrix.c2.y, matrix.c2.z, matrix.c2.w,
                matrix.c3.x, matrix.c3.y, matrix.c3.z, matrix.c3.w,
            };
        }
    }
}
