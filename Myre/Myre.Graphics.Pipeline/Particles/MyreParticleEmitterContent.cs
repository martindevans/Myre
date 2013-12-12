using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;
using Myre.Graphics.Pipeline.Particles.Initialisers;
using Myre.Graphics.Pipeline.Particles.Triggers;

namespace Myre.Graphics.Pipeline.Particles
{
    public class MyreParticleEmitterContent
    {
        public readonly string System;
        public readonly ITrigger[] Triggers;
        public readonly IInitialiser[] Initialisers;

        public MyreParticleEmitterContent(string system, ITrigger[] triggers, IInitialiser[] initialisers)
        {
            System = system;
            Triggers = triggers;
            Initialisers = initialisers;
        }
    }

    [ContentTypeWriter]
    public class MyreParticleEmitterContentWriter : ContentTypeWriter<MyreParticleEmitterContent>
    {
        protected override void Write(ContentWriter output, MyreParticleEmitterContent value)
        {
            output.Write(value.System);

            output.Write(value.Triggers.Length);
            for (int i = 0; i < value.Triggers.Length; i++)
                output.WriteObject(value.Triggers[i]);

            output.Write(value.Initialisers.Length);
            for (int i = 0; i < value.Initialisers.Length; i++)
                output.WriteObject(value.Initialisers[i]);
        }

        public override string GetRuntimeReader(TargetPlatform targetPlatform)
        {
            return "Myre.Graphics.Translucency.Particles.ParticleEmitterDescriptionReader, Myre.Graphics";
        }

        public override string GetRuntimeType(TargetPlatform targetPlatform)
        {
            return "Myre.Graphics.Translucency.Particles.ParticleEmitterDescription, Myre.Graphics";
        }
    }
}
