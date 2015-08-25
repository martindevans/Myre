using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework.Graphics;
using System.Numerics;

namespace Myre.Extensions
{
    public static class EffectParameterExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetValue(this EffectParameter collection, Matrix4x4 value)
        {
            collection.SetValue(value.ToXNA());
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetValue(this EffectParameter collection, Vector2 value)
        {
            collection.SetValue(value.ToXNA());
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetValue(this EffectParameter collection, Vector3 value)
        {
            collection.SetValue(value.ToXNA());
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetValue(this EffectParameter collection, Vector4 value)
        {
            collection.SetValue(value.ToXNA());
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetValue(this EffectParameter collection, Quaternion value)
        {
            collection.SetValue(value.ToXNA());
        }
    }
}
