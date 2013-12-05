using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;

namespace Myre.Graphics.Pipeline.Particles.Initialisers.Velocity
{
    public class RandomVelocity
        :IInitialiser
    {
        public Vector3 Max { get; set; }
    }

    [ContentTypeWriter]
    public class RandomVelocityWriter
        : ContentTypeWriter<RandomVelocity>
    {
        protected override void Write(ContentWriter output, RandomVelocity value)
        {
            output.Write(value.Max);
        }

        public override string GetRuntimeType(TargetPlatform targetPlatform)
        {
            return "Myre.Graphics.Translucency.Particles.Initialisers.Velocity.RandomVelocity, Myre.Graphics";
        }

        public override string GetRuntimeReader(TargetPlatform targetPlatform)
        {
            return "Myre.Graphics.Translucency.Particles.Initialisers.Velocity.RandomVelocityReader, Myre.Graphics";
        }
    }
}
