using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;
using Microsoft.Xna.Framework.Graphics;
using Myre.Graphics.Pipeline.Particles.Initialisers;
using Myre.Graphics.Pipeline.Particles.Triggers;

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
        public readonly ITrigger[] Triggers;
        public readonly IInitialiser[] Initialisers;

        public MyreParticleSystemDescriptionContent(BlendState blendState, float endLinearVelocity, float endScale, Vector3 gravity, float lifetime, string texture, int capacity, ITrigger[] triggers, IInitialiser[] initialisers)
        {
            BlendState = blendState;
            EndLinearVelocity = endLinearVelocity;
            EndScale = endScale;
            Gravity = gravity;
            Lifetime = lifetime;
            Texture = texture;
            Capacity = capacity;
            Triggers = triggers;
            Initialisers = initialisers;
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

            output.Write(value.Triggers.Length);
            for (int i = 0; i < value.Triggers.Length; i++)
                output.WriteObject(value.Triggers[i]);

            output.Write(value.Initialisers.Length);
            for (int i = 0; i < value.Initialisers.Length; i++)
                output.WriteObject(value.Initialisers[i]);
        }

        public override string GetRuntimeReader(TargetPlatform targetPlatform)
        {
            return "Myre.Graphics.Translucency.Particles.ParticleSystemGeneratorReader, Myre.Graphics";
        }

        public override string GetRuntimeType(TargetPlatform targetPlatform)
        {
            return "Myre.Graphics.Translucency.Particles.ParticleSystemGenerator, Myre.Graphics";
        }
    }
}
