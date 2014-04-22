using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;
using Microsoft.Xna.Framework.Graphics;

namespace Myre.Graphics.Pipeline.Particles
{
    [ContentSerializerRuntimeType("Myre.Graphics.Particles.ParticleSystemDescription, Myre.Graphics")]
    public class MyreParticleSystemDescriptionContent
    {
        public readonly BlendState BlendState;
        public readonly float EndLinearVelocity;
        public readonly float EndScale;
        public readonly Vector3 Gravity;
        public readonly float Lifetime;
        public readonly string Texture;
        public readonly int Capacity;
        public readonly string Type;

        public MyreParticleSystemDescriptionContent(BlendState blendState, float endLinearVelocity, float endScale, Vector3 gravity, float lifetime, string texture, int capacity, string type)
        {
            BlendState = blendState;
            EndLinearVelocity = endLinearVelocity;
            EndScale = endScale;
            Gravity = gravity;
            Lifetime = lifetime;
            Texture = texture;
            Capacity = capacity;
            Type = type;
        }
    }

    [ContentTypeWriter]
    public class MyreParticleSystemDefinitionContentWriter : ContentTypeWriter<MyreParticleSystemDescriptionContent>
    {
        protected override void Write(ContentWriter output, MyreParticleSystemDescriptionContent value)
        {
            output.WriteObject(value.BlendState);
            output.Write(value.EndLinearVelocity);
            output.Write(value.EndScale);
            output.Write(value.Gravity);
            output.Write(value.Lifetime);
            output.Write(value.Texture);
            output.Write(value.Capacity);
            output.Write(value.Type);
        }

        public override string GetRuntimeReader(TargetPlatform targetPlatform)
        {
            return "Myre.Graphics.Translucency.Particles.ParticleSystemDescriptionReader, Myre.Graphics";
        }

        public override string GetRuntimeType(TargetPlatform targetPlatform)
        {
            return "Myre.Graphics.Translucency.Particles.ParticleSystemDescription, Myre.Graphics";
        }
    }
}
