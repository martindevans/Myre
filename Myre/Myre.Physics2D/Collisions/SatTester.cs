using Microsoft.Xna.Framework;

namespace Myre.Physics2D.Collisions
{
    public class SatTester
    {
        public SatResult? FindIntersection(Geometry a, Geometry b)
        {
            var aAxes = a.GetAxes(b);
            var bAxes = b.GetAxes(a);
            var totalAxes = aAxes.Length + bAxes.Length;

            Vector2 normal = Vector2.Zero;
            Vector2 deepestPoint = Vector2.Zero;
            float smallestOverlap = float.MaxValue;
            
            for (int i = 0; i < totalAxes; i++)
            {
                Vector2 axis;
                if (i < aAxes.Length)
                    axis = aAxes[i];
                else
                    axis = bAxes[i - aAxes.Length];

                Vector2.Normalize(ref axis, out axis);

                var aProjection = a.Project(axis);
                var bProjection = b.Project(axis);

                float overlap;
                int order;
                if (!aProjection.Overlaps(bProjection, out overlap, out order))
                    return null;

                if (overlap < smallestOverlap)
                {
                    smallestOverlap = overlap;
                    normal = axis;

                    if (order < 0)
                        normal = -normal;

                    if (i < aAxes.Length)
                    {
                        if (order > 0) // if a is before b
                            deepestPoint = bProjection.StartPoint;
                        else
                            deepestPoint = bProjection.EndPoint;
                    }
                    else
                    {
                        if (order > 0)
                            deepestPoint = aProjection.EndPoint;
                        else
                            deepestPoint = aProjection.StartPoint;
                    }
                }
            }

            var axisMagnitude = normal.Length();
            normal /= axisMagnitude;
            smallestOverlap /= axisMagnitude;

            return new SatResult(a, b, normal, smallestOverlap, deepestPoint);
        }
    }
}
