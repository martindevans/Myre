using Microsoft.Xna.Framework;

namespace Myre.Graphics.Geometry
{
    public interface ICullable
    {
        BoundingSphere Bounds { get; }
        //OrientedBoundingBox BoundingBox { get; }
        //bool IsVisble { get;  set; }
    }
}
