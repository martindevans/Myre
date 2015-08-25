using System;
using System.Numerics;
using Myre.Extensions;

namespace Myre.Graphics.Animation
{
    [Serializable]
    public struct Transform
    {
        public static readonly Transform Identity = new Transform { Rotation = Quaternion.Identity, Scale = Vector3.One, Translation = Vector3.Zero };

        public Vector3 Translation;
        public Vector3 Scale;
        public Quaternion Rotation;

        public Transform Interpolate(Transform b, float amount)
        {
            Transform result;
            Interpolate(ref b, amount, out result);
            return result;
        }

        public void Interpolate(ref Transform b, float amount, out Transform result)
        {
            result.Translation = Vector3.Lerp(Translation, b.Translation, amount);
            Rotation.Nlerp(ref b.Rotation, amount, out result.Rotation);
            result.Scale = Vector3.Lerp(Scale, b.Scale, amount);
        }

        /// <summary>
        /// Calculates the transform that moves from A to B
        /// </summary>
        /// <param name="b"></param>
        /// <param name="a"></param>
        /// <returns></returns>
        public static Transform Subtract(Transform b, Transform a)
        {
            return new Transform
            {
                Scale = b.Scale - a.Scale,
                Translation = b.Translation - a.Translation,
                Rotation = b.Rotation * Quaternion.Inverse(a.Rotation)
            };
        }

        public void ToMatrix(out Matrix4x4 matrix)
        {
            Matrix4x4 s = Matrix4x4.CreateScale(Scale);
            Matrix4x4 r = Matrix4x4.CreateFromQuaternion(Rotation);
            Matrix4x4 t = Matrix4x4.CreateTranslation(Translation);

            matrix = Matrix4x4.Multiply(s, r);
            matrix = Matrix4x4.Multiply(matrix, t);
        }

        public Matrix4x4 ToMatrix()
        {
            Matrix4x4 m;
            ToMatrix(out m);
            return m;
        }
    }
}
