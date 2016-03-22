using System.Diagnostics.Contracts;
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
            Contract.Requires(collection != null);

            collection.SetValue(value.ToXNA());
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetValue(this EffectParameter collection, Vector2 value)
        {
            Contract.Requires(collection != null);

            collection.SetValue(value.ToXNA());
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetValue(this EffectParameter collection, Vector3 value)
        {
            Contract.Requires(collection != null);

            collection.SetValue(value.ToXNA());
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetValue(this EffectParameter collection, Vector4 value)
        {
            Contract.Requires(collection != null);

            collection.SetValue(value.ToXNA());
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetValue(this EffectParameter collection, Quaternion value)
        {
            Contract.Requires(collection != null);

            collection.SetValue(value.ToXNA());
        }
    }
}
