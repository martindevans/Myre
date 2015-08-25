using System.Collections.Generic;
using System.Numerics;

using ContainmentType = Microsoft.Xna.Framework.ContainmentType;

namespace Myre.Graphics.Geometry
{
    public class BoundingVolume
        : List<Plane>
    {
        private readonly Vector3[] _corners = new Vector3[8];

        public BoundingVolume()
        {
        }

        public BoundingVolume(params Plane[] planes)
            : base(planes)
        {
        }

        public void Add(BoundingFrustum frustum)
        {
            Add(Flip(frustum.Near));
            Add(Flip(frustum.Left));
            Add(Flip(frustum.Right));
            Add(Flip(frustum.Bottom));
            Add(Flip(frustum.Far));
            Add(Flip(frustum.Top));
        }

        private static Plane Flip(Plane plane)
        {
            return new Plane(-plane.Normal, -plane.D);
        }

        public void Add(BoundingBox box)
        {
            Add(new Plane(Vector3.UnitY, box.Min.Y));
            Add(new Plane(-Vector3.UnitY, -box.Max.Y));
            Add(new Plane(-Vector3.UnitX, -box.Max.X));
            Add(new Plane(Vector3.UnitX, box.Min.X));
            Add(new Plane(-Vector3.UnitZ, -box.Max.Z));
            Add(new Plane(Vector3.UnitZ, box.Min.Z));
        }

        public bool Intersects(Vector3 point)
        {
            for (int i = 0; i < Count; i++)
            {
                var plane = this[i];
                float distance = Plane.DotCoordinate(plane, point);

                if (distance < 0)
                    return false;
            }

            return true;
        }

        public bool Intersects(BoundingSphere sphere)
        {
            for (int i = 0; i < Count; i++)
            {
                var plane = this[i];
                float distance = Vector3.Dot(plane.Normal, sphere.Center);

                if (-plane.D - distance > sphere.Radius)
                    return false;
            }

            return true;
        }

        public bool Intersects(BoundingBox box)
        {
            return Intersects(new Vector3(box.Min.X, box.Min.Y, box.Min.Z))
                && Intersects(new Vector3(box.Max.X, box.Min.Y, box.Min.Z))
                && Intersects(new Vector3(box.Min.X, box.Max.Y, box.Min.Z))
                && Intersects(new Vector3(box.Min.X, box.Min.Y, box.Max.Z))
                && Intersects(new Vector3(box.Max.X, box.Max.Y, box.Min.Z))
                && Intersects(new Vector3(box.Min.X, box.Max.Y, box.Max.Z))
                && Intersects(new Vector3(box.Max.X, box.Min.Y, box.Max.Z))
                && Intersects(new Vector3(box.Max.X, box.Max.Y, box.Max.Z));
        }

        public ContainmentType Contains(Vector3 point)
        {
            if (Intersects(point))
                return ContainmentType.Contains;

            return ContainmentType.Disjoint;
        }

        public ContainmentType Contains(BoundingSphere sphere)
        {
            var containment = ContainmentType.Contains;

            for (int i = 0; i < Count; i++)
            {
                float distance = Plane.DotCoordinate(this[i], sphere.Center);

                if (distance < sphere.Radius)
                {
                    if (distance < -sphere.Radius)
                        return ContainmentType.Disjoint;
                    else
                        containment = ContainmentType.Intersects;
                }
            }

            return containment;
        }

        public ContainmentType Contains(BoundingBox box)
        {
            var outside = 0;

            box.GetCorners(_corners);
            for (int i = 0; i < _corners.Length; i++)
            {
                if (!Intersects(_corners[i]))
                    outside++;

                if (outside > 0 && outside != i)
                    return ContainmentType.Intersects;
            }

            return (outside == _corners.Length) ? ContainmentType.Disjoint : ContainmentType.Contains;
        }
    }
}
