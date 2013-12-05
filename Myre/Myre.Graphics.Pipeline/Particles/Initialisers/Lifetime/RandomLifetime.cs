using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;

namespace Myre.Graphics.Pipeline.Particles.Initialisers.Lifetime
{
    public class RandomLifetime
        :IInitialiser
    {
        public float Min { get; set; }
        public float Max { get; set; }
    }

    [ContentTypeWriter]
    public class RandomLifetimeWriter
        : ContentTypeWriter<RandomLifetime>
    {
        protected override void Write(ContentWriter output, RandomLifetime value)
        {
            output.Write(value.Min);
            output.Write(value.Max);
        }

        public override string GetRuntimeType(TargetPlatform targetPlatform)
        {
            return "Myre.Graphics.Translucency.Particles.Initialisers.Lifetime.RandomLifetime, Myre.Graphics";
        }

        public override string GetRuntimeReader(TargetPlatform targetPlatform)
        {
            return "Myre.Graphics.Translucency.Particles.Initialisers.Lifetime.RandomLifetimeReader, Myre.Graphics";
        }
    }
}
