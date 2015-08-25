using System.Numerics;
using Myre.Graphics.Animation;

namespace Myre.Graphics.Extensions
{
    public static class MatrixExtensions
    {
        public static Transform? ToTransform(this Matrix4x4 m)
        {
            Vector3 scale, translation;
            Quaternion rotation;
            if (!Matrix4x4.Decompose(m, out scale, out rotation, out translation))
                return null;

            return new Transform { Rotation = rotation, Scale = scale, Translation = translation };
        }
    }
}
