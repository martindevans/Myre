using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;

namespace Myre.Graphics.Pipeline.Particles
{
    [ContentProcessor(DisplayName = "Myre Particle System Processor")]
    public class MyreParticleSystemProcessor : ContentProcessor<ParticleSystemDescription, MyreParticleSystemDescriptionContent>
    {
        public override MyreParticleSystemDescriptionContent Process(ParticleSystemDescription input, ContentProcessorContext context)
        {
            var r = new ExternalReference<TextureContent>(input.Texture);
            context.AddDependency(r.Filename);

            return new MyreParticleSystemDescriptionContent(input.BlendState, input.EndLinearVelocity, input.EndScale, input.Gravity, input.Lifetime, input.Texture, input.Capacity, input.Triggers, input.Initialisers);
        }
    }
}
