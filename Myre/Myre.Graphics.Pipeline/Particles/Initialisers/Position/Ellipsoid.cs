using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;

namespace Myre.Graphics.Pipeline.Particles.Initialisers.Position
{
    public class Ellipsoid
        :IInitialiser
    {
        public Vector3 Shape { get; set; }
        public float MinDistance { get; set; }
    }

    [ContentTypeWriter]
    public class EllipsoidWriter
        : ContentTypeWriter<Ellipsoid>
    {
        protected override void Write(ContentWriter output, Ellipsoid value)
        {
            output.Write(value.Shape);
            output.Write(value.MinDistance);
        }

        public override string GetRuntimeType(TargetPlatform targetPlatform)
        {
            return "Myre.Graphics.Translucency.Particles.Initialisers.Position.Ellipsoid, Myre.Graphics";
        }

        public override string GetRuntimeReader(TargetPlatform targetPlatform)
        {
            return "Myre.Graphics.Translucency.Particles.Initialisers.Position.EllipsoidReader, Myre.Graphics";
        }
    }
}
