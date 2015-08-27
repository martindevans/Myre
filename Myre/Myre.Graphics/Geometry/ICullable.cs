
using SwizzleMyVectors.Geometry;

namespace Myre.Graphics.Geometry
{
    public interface ICullable
    {
        BoundingSphere Bounds { get; }
    }
}
