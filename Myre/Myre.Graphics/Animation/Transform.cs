using System;
using System.Diagnostics.Contracts;
using Microsoft.Xna.Framework;
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

        [Pure]
        public Transform Interpolate(Transform b, float amount)
        {
            Transform result;
            Interpolate(ref b, amount, out result);
            return result;
        }

        public void Interpolate(ref Transform b, float amount, out Transform result)
        {
            Vector3.Lerp(ref Translation, ref b.Translation, amount, out result.Translation);
            Rotation.Nlerp(ref b.Rotation, amount, out result.Rotation);
            Vector3.Lerp(ref Scale, ref b.Scale, amount, out result.Scale);
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

        public void ToMatrix(out Matrix matrix)
        {
            Matrix s;
            Matrix.CreateScale(ref Scale, out s);

            Matrix r;
            Matrix.CreateFromQuaternion(ref Rotation, out r);

            Matrix t;
            Matrix.CreateTranslation(ref Translation, out t);

            Matrix.Multiply(ref s, ref r, out matrix);
            Matrix.Multiply(ref matrix, ref t, out matrix);
        }

        public Matrix ToMatrix()
        {
            Matrix m;
            ToMatrix(out m);
            return m;
        }
    }
}
