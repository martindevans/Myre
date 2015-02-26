using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Myre.Graphics.Geometry
{
    public struct VertexPosition
        : IVertexType
    {
        private static readonly VertexDeclaration _vertexDeclaration = new VertexDeclaration(
            new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0)
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
        public Vector3 Position;
// ReSharper restore NotAccessedField.Global
// ReSharper restore MemberCanBePrivate.Global

        public VertexPosition(Vector3 position)
        {
            Position = position;
        }

        public VertexPosition(float x, float y, float z)
        {
            Position = new Vector3(x, y, z);
        }
    }
}
