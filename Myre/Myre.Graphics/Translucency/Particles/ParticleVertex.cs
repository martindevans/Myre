using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Graphics.PackedVector;

namespace Myre.Graphics.Translucency.Particles
{
    struct ParticleVertex
    {
        public Short2 Corner;
        public Vector3 Position;
        public Vector4 Velocity;
        public Color StartColour;
        public Color EndColour;
        public HalfVector2 Scales;
        public float Time;

        public static readonly VertexDeclaration VertexDeclaration = new VertexDeclaration
        (
            // Corner
            new VertexElement(0, VertexElementFormat.Short2,
                                 VertexElementUsage.Position, 0),
            // Position
            new VertexElement(4, VertexElementFormat.Vector3,
                                 VertexElementUsage.Position, 1),
            // Velocity
            new VertexElement(16, VertexElementFormat.Vector4,
                                  VertexElementUsage.Normal, 0),
            // StartColour
            new VertexElement(32, VertexElementFormat.Color,
                                  VertexElementUsage.Color, 0),
            // EndColour
            new VertexElement(36, VertexElementFormat.Color,
                                  VertexElementUsage.Color, 1),
            // Scales
            new VertexElement(40, VertexElementFormat.HalfVector2,
                                  VertexElementUsage.TextureCoordinate, 0),
            // Time
            new VertexElement(44, VertexElementFormat.Single,
                                  VertexElementUsage.TextureCoordinate, 1)
        );

        public const int SIZE_IN_BYTES = 48;
    }
}
