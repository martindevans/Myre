
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;

namespace Myre.Graphics.Pipeline.Particles.Initialisers.AngularVelocity
{
    public class RandomAngularVelocity
        :IInitialiser
    {
        public float Min { get; set; }
        public float Max { get; set; }
    }

    [ContentTypeWriter]
    public class RandomAngularVelocityWriter
        :ContentTypeWriter<RandomAngularVelocity>
    {
        protected override void Write(ContentWriter output, RandomAngularVelocity value)
        {
            output.Write(value.Min);
            output.Write(value.Max);
        }

        public override string GetRuntimeType(TargetPlatform targetPlatform)
        {
            return "Myre.Graphics.Translucency.Particles.Initialisers.AngularVelocity.RandomAngularVelocity, Myre.Graphics";
        }

        public override string GetRuntimeReader(TargetPlatform targetPlatform)
        {
            return "Myre.Graphics.Translucency.Particles.Initialisers.AngularVelocity.RandomAngularVelocityReader, Myre.Graphics";
        }
    }
}
