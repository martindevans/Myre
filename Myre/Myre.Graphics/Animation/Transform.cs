using Microsoft.Xna.Framework;
using Myre.Extensions;

namespace Myre.Graphics.Animation
{
    public struct Transform
    {
        public static readonly Transform Identity = new Transform { Rotation = Quaternion.Identity, Scale = Vector3.One, Translation = Vector3.Zero };

        public Vector3 Translation;
        public Vector3 Scale;
        public Quaternion Rotation;

        public Transform Interpolate(Transform b, float amount)
        {
            return new Transform
            {
                Translation = Vector3.Lerp(Translation, b.Translation, amount),
                Rotation = Rotation.Nlerp(b.Rotation, amount),
                Scale = Vector3.Lerp(Scale, b.Scale, amount)
            };
        }

        /// <summary>
        /// Calculates the transform that moves from A to B
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static Transform Difference(Transform a, Transform b)
        {
            return new Transform
            {
                Scale = b.Scale - a.Scale,
                Translation = b.Translation - a.Translation,
                Rotation = b.Rotation * Quaternion.Inverse(a.Rotation)
            };
        }
    }
}
