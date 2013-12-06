
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;

namespace Myre.Graphics.Pipeline.Particles.Triggers
{
    public class Burst
        :ITrigger
    {
        public float BurstLength { get; set; }
        public int BurstParticles { get; set; }
        public float BurstDelay { get; set; }
    }

    [ContentTypeWriter]
    public class BurstWriter
        : ContentTypeWriter<Burst>
    {
        protected override void Write(ContentWriter output, Burst value)
        {
            output.Write(value.BurstLength);
            output.Write(value.BurstParticles);
            output.Write(value.BurstDelay);
        }

        public override string GetRuntimeType(TargetPlatform targetPlatform)
        {
            return "Myre.Graphics.Translucency.Particles.Triggers.Burst, Myre.Graphics";
        }

        public override string GetRuntimeReader(TargetPlatform targetPlatform)
        {
            return "Myre.Graphics.Translucency.Particles.Triggers.BurstReader, Myre.Graphics";
        }
    }
}
