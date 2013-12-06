using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;

namespace Myre.Graphics.Pipeline.Particles.Initialisers.Position
{
    public class InterpolatedEntityPosition
        :IInitialiser
    {
        public int BatchSize { get; set; }
    }

    [ContentTypeWriter]
    public class InterpolatedEntityPositionWriter
        : ContentTypeWriter<InterpolatedEntityPosition>
    {
        protected override void Write(ContentWriter output, InterpolatedEntityPosition value)
        {
            output.Write(value.BatchSize);
        }

        public override string GetRuntimeType(TargetPlatform targetPlatform)
        {
            return "Myre.Graphics.Translucency.Particles.Initialisers.Position.InterpolatedEntityPosition, Myre.Graphics";
        }

        public override string GetRuntimeReader(TargetPlatform targetPlatform)
        {
            return "Myre.Graphics.Translucency.Particles.Initialisers.Position.InterpolatedEntityPositionReader, Myre.Graphics";
        }
    }
}
