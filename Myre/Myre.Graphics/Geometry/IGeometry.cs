using System.Numerics;
using Myre.Graphics.Materials;
using SwizzleMyVectors.Geometry;

namespace Myre.Graphics.Geometry
{
    public interface IGeometry
    {
        Matrix4x4 WorldView { get; }

        BoundingSphere BoundingSphere { get; }

        void Draw(Material material, Renderer renderer);

        void Draw(string phase, Renderer renderer);
    }
}
