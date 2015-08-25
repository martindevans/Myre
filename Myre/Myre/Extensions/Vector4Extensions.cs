using System.Numerics;
using System.Runtime.CompilerServices;

namespace Myre.Extensions
{
    public static class Vector4Extensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Microsoft.Xna.Framework.Vector4 ToXNA(this Vector4 vector)
        {
            return new Microsoft.Xna.Framework.Vector4(vector.X, vector.Y, vector.Z, vector.W);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector4 FromXNA(this Microsoft.Xna.Framework.Vector4 vector)
        {
            return new Vector4(vector.X, vector.Y, vector.Z, vector.W);
        }
    }
}
