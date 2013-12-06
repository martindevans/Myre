
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;

namespace Myre.Graphics.Pipeline.Particles.Initialisers.Velocity
{
    public class EntityVelocity
        :IInitialiser
    {
        public float VelocityBleedThrough { get; set; }
    }

    [ContentTypeWriter]
    public class EllipsoidWriter
        : ContentTypeWriter<EntityVelocity>
    {
        protected override void Write(ContentWriter output, EntityVelocity value)
        {
            output.Write(value.VelocityBleedThrough);
        }

        public override string GetRuntimeType(TargetPlatform targetPlatform)
        {
            return "Myre.Graphics.Translucency.Particles.Initialisers.Velocity.EntityVelocity, Myre.Graphics";
        }

        public override string GetRuntimeReader(TargetPlatform targetPlatform)
        {
            return "Myre.Graphics.Translucency.Particles.Initialisers.Velocity.EntityVelocityReader, Myre.Graphics";
        }
    }
}
