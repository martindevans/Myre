using Microsoft.Xna.Framework;

namespace Myre.Physics2D.Collisions
{
    public struct SatResult
    {
        public readonly Geometry A;
        public readonly Geometry B;
        public readonly Vector2 NormalAxis;
        public float Penetration;
        public Vector2 DeepestPoint;

        public SatResult(Geometry a, Geometry b, Vector2 normalAxis, float penetration, Vector2 deepestPoint)
        {
            A = a;
            B = b;
            NormalAxis = normalAxis;
            Penetration = penetration;
            DeepestPoint = deepestPoint;
        }

        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        public bool Equals(SatResult obj)
        {
            return A == obj.A
                && B == obj.B
                && NormalAxis == obj.NormalAxis
                && Penetration == obj.Penetration
                && DeepestPoint == obj.DeepestPoint;
        }

        public override int GetHashCode()
        {
            return A.GetHashCode() ^ B.GetHashCode() ^ NormalAxis.GetHashCode();
        }
    }
}
