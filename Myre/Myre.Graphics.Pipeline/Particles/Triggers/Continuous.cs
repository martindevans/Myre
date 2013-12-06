using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;

namespace Myre.Graphics.Pipeline.Particles.Triggers
{
    public class Continuous
        :ITrigger
    {
        public float EmitsPerSecond { get; set; }
    }

    [ContentTypeWriter]
    public class ContinuousWriter
        : ContentTypeWriter<Continuous>
    {
        protected override void Write(ContentWriter output, Continuous value)
        {
            output.Write(value.EmitsPerSecond);
        }

        public override string GetRuntimeType(TargetPlatform targetPlatform)
        {
            return "Myre.Graphics.Translucency.Particles.Triggers.Continuous, Myre.Graphics";
        }

        public override string GetRuntimeReader(TargetPlatform targetPlatform)
        {
            return "Myre.Graphics.Translucency.Particles.Triggers.ContinuousReader, Myre.Graphics";
        }
    }
}
