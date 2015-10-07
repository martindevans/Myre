using System.Numerics;
using Myre.Graphics.Materials;
using BoundingSphere = SwizzleMyVectors.Geometry.BoundingSphere;

namespace Myre.Graphics.Geometry
{
    public interface IGeometry
    {
        Matrix4x4 WorldView { get; }

        Matrix4x4 World { get; }

        BoundingSphere BoundingSphere { get; }

        void Draw(Material material, Renderer renderer);

        void Draw(string phase, Renderer renderer);
    }
}
