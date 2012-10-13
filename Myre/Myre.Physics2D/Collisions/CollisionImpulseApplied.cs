using Microsoft.Xna.Framework;

namespace Myre.Physics2D.Collisions
{
    public struct CollisionImpulseApplied
    {
        public readonly Geometry A;
        public readonly Geometry B;

        public readonly Vector2 Impulse;

        public CollisionImpulseApplied(Geometry a, Geometry b, Vector2 impulse)
        {
            A = a;
            B = b;
            Impulse = impulse;
        }
    }
}
