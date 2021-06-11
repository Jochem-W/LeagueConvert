using System.Collections.Generic;
using System.Numerics;

namespace SimpleGltf.Extensions
{
    public static class Matrix4X4Extensions
    {
        public static IEnumerable<float> GetValues(this Matrix4x4 matrix)
        {
            return new[]
            {
                matrix.M11, matrix.M12, matrix.M13, matrix.M14,
                matrix.M21, matrix.M22, matrix.M23, matrix.M24,
                matrix.M31, matrix.M32, matrix.M33, matrix.M34,
                matrix.M41, matrix.M42, matrix.M43, matrix.M44
            };
        }

        public static Matrix4x4 FixInverseBindMatrix(this Matrix4x4 matrix)
        {
            return matrix * (1 / matrix.M44);
        }

        public static Matrix4x4 Transpose(this Matrix4x4 matrix)
        {
            return Matrix4x4.Transpose(matrix);
        }
    }
}