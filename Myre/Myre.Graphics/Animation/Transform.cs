using Microsoft.Xna.Framework;
using Myre.Extensions;

namespace Myre.Graphics.Animation
{
    public struct Transform
    {
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
    }
}
