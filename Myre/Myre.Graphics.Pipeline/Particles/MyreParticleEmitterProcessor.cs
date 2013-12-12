using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;

namespace Myre.Graphics.Pipeline.Particles
{
    [ContentProcessor(DisplayName = "Myre Particle Emitter Processor")]
    public class MyreParticleEmitterProcessor : ContentProcessor<ParticleEmitter, MyreParticleEmitterContent>
    {
        public override MyreParticleEmitterContent Process(ParticleEmitter input, ContentProcessorContext context)
        {
            var r = new ExternalReference<TextureContent>(input.System);
            context.AddDependency(r.Filename);

            return new MyreParticleEmitterContent(input.System, input.Triggers, input.Initialisers);
        }
    }
}
