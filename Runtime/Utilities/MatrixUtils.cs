// Copyright 2022-2026 Niantic Spatial.

using UnityEngine;

namespace NianticSpatial.NSDK.AR
{
    /// A utility class to help grepping information from matrices.
    public static class MatrixUtils
    {
        /// <summary>
        /// Returns the position from a transform matrix.
        /// </summary>
        /// <param name="matrix">The matrix from which to extract the position.</param>
        public static Vector3 PositionFromMatrix(Matrix4x4 matrix)
        {
            Vector3 position = matrix.GetColumn(3);
            return position;
        }

        public static Vector3 ToPosition(this Matrix4x4 matrix)
        {
            return PositionFromMatrix(matrix);
        }

        /// <summary>
        /// Returns the rotation as a quaternion from a transform matrix
        /// @note This does not work on matrices with negative scale values.
        /// </summary>
        /// <param name="matrix">The matrix from which to extract the rotation.</param>
        public static Quaternion RotationFromMatrix(Matrix4x4 matrix)
        {
            // Adapted from https://answers.unity.com/questions/402280/how-to-decompose-a-trs-matrix.html
            return Quaternion.LookRotation(matrix.GetColumn(2), matrix.GetColumn(1));
        }

        /// <summary>
        /// Returns the rotation as a quaternion from a transform matrix
        /// @note This does not work on matrices with negative scaled values.
        /// </summary>
        /// <param name="matrix">The matrix from which to extract the rotation.</param>
        public static Quaternion ToRotation(this Matrix4x4 matrix)
        {
            return RotationFromMatrix(matrix);
        }

        public static Matrix4x4 InvalidMatrix
        {
            get
            {
                return new Matrix4x4
                (
                    new Vector4(float.NaN, float.NaN, float.NaN, float.NaN),
                    new Vector4(float.NaN, float.NaN, float.NaN, float.NaN),
                    new Vector4(float.NaN, float.NaN, float.NaN, float.NaN),
                    new Vector4(float.NaN, float.NaN, float.NaN, float.NaN)
                );
            }
        }
    }
}
