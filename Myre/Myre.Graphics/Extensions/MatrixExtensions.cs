using Microsoft.Xna.Framework;
using Myre.Graphics.Animation;

namespace Myre.Graphics.Extensions
{
    public static class MatrixExtensions
    {
        public static Transform? ToTransform(this Matrix m)
        {
            Vector3 scale, translation;
            Quaternion rotation;
            if (!m.Decompose(out scale, out rotation, out translation))
                return null;

            return new Transform { Rotation = rotation, Scale = scale, Translation = translation };
        }
    }
}
