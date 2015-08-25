using System.Numerics;
using System.Runtime.CompilerServices;

namespace Myre.Extensions
{
    // ReSharper disable once InconsistentNaming
    public static class Matrix4x4Extensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Microsoft.Xna.Framework.Matrix ToXNA(this Matrix4x4 matrix)
        {
            return new Microsoft.Xna.Framework.Matrix(
                matrix.M11, matrix.M12, matrix.M13, matrix.M14,
                matrix.M21, matrix.M22, matrix.M23, matrix.M24,
                matrix.M31, matrix.M32, matrix.M33, matrix.M34,
                matrix.M41, matrix.M42, matrix.M43, matrix.M44
            );
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Matrix4x4 FromXNA(this Microsoft.Xna.Framework.Matrix matrix)
        {
            return new Matrix4x4(
                matrix.M11, matrix.M12, matrix.M13, matrix.M14,
                matrix.M21, matrix.M22, matrix.M23, matrix.M24,
                matrix.M31, matrix.M32, matrix.M33, matrix.M34,
                matrix.M41, matrix.M42, matrix.M43, matrix.M44
            );
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 Up(this Matrix4x4 matrix)
        {
            return new Vector3(matrix.M21, matrix.M22, matrix.M23);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Matrix4x4 Up(this Matrix4x4 matrix, Vector3 value)
        {
            matrix.M21 = value.X;
            matrix.M22 = value.Y;
            matrix.M23 = value.Z;

            return matrix;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 Down(this Matrix4x4 matrix)
        {
            return -matrix.Up();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Matrix4x4 Down(this Matrix4x4 matrix, Vector3 value)
        {
            return Up(matrix, -value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 Left(this Matrix4x4 matrix)
        {
            return -matrix.Right();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Matrix4x4 Left(this Matrix4x4 matrix, Vector3 value)
        {
            return Right(matrix, -value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 Right(this Matrix4x4 matrix)
        {
            return new Vector3(matrix.M11, matrix.M12, matrix.M13);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Matrix4x4 Right(this Matrix4x4 matrix, Vector3 value)
        {
            matrix.M11 = value.X;
            matrix.M12 = value.Y;
            matrix.M13 = value.Z;

            return matrix;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 Forward(this Matrix4x4 matrix)
        {
            return new Vector3(-matrix.M31, -matrix.M32, -matrix.M33);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Matrix4x4 Forward(this Matrix4x4 matrix, Vector3 value)
        {
            matrix.M31 = -value.X;
            matrix.M32 = -value.Y;
            matrix.M33 = -value.Z;

            return matrix;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 Backward(this Matrix4x4 matrix)
        {
            return -matrix.Forward();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Matrix4x4 Backward(this Matrix4x4 matrix, Vector3 value)
        {
            return Forward(matrix, -value);
        }
    }
}
