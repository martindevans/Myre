using System;
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

        public Transform Interpolate(Transform b, float amount)
        {
            Transform result;
            Interpolate(ref b, amount, out result);
            return result;
        }

        public void Interpolate(ref Transform b, float amount, out Transform result)
        {
            result = new Transform
            {
                Translation = Vector3.Lerp(Translation, b.Translation, amount),
                Rotation = Rotation.Nlerp(b.Rotation, amount),
                Scale = Vector3.Lerp(Scale, b.Scale, amount)
            };
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
    }
}
