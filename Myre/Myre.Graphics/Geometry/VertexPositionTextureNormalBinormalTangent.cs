using System.Numerics;
using Microsoft.Xna.Framework.Graphics;
using Myre.Extensions;

namespace Myre.Graphics.Geometry
{
    /// <summary>
    /// Defines the vertex format Myre wants for default Gbuffer rendering. If you define your own Vertex type, include at least this information
    /// </summary>
    public struct VertexPositionTextureNormalBinormalTangent
        : IVertexType
    {
        private static readonly VertexDeclaration _vertexDeclaration = new VertexDeclaration(
                new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0),
                new VertexElement(12, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 0),
                new VertexElement(20, VertexElementFormat.Vector3, VertexElementUsage.Normal, 0),
                new VertexElement(32, VertexElementFormat.Vector3, VertexElementUsage.Binormal, 0),
                new VertexElement(44, VertexElementFormat.Vector3, VertexElementUsage.Tangent, 0)
            );

        public VertexDeclaration VertexDeclaration
        {
            get
            {
                return _vertexDeclaration;
            }
        }

        public static int SizeInBytes
        {
            get { return _vertexDeclaration.VertexStride; }
        }

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable NotAccessedField.Global
        public Microsoft.Xna.Framework.Vector3 Position;
        public Microsoft.Xna.Framework.Vector2 TextureCoordinate;
        public Microsoft.Xna.Framework.Vector3 Normal;
        public Microsoft.Xna.Framework.Vector3 Binormal;
        public Microsoft.Xna.Framework.Vector3 Tangent;
// ReSharper restore NotAccessedField.Global
// ReSharper restore MemberCanBePrivate.Global

        public VertexPositionTextureNormalBinormalTangent(Vector3 position, Vector2 textureCoordinate, Vector3 normal, Vector3 binormal, Vector3 tangent)
        {
            Position = position.ToXNA();
            TextureCoordinate = textureCoordinate.ToXNA();
            Normal = normal.ToXNA();
            Binormal = binormal.ToXNA();
            Tangent = tangent.ToXNA();
        }
    }
}
