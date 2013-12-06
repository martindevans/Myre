using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Myre.Graphics.Translucency.Particles.Initialisers;

namespace Myre.Graphics.Translucency.Particles
{
    public class ParticleSystemGenerator
    {
        private readonly BaseParticleInitialiser[] _initialisers;
        public ParticleSystemDescription Description;

        public ParticleSystemGenerator(BlendState blend, float endLinearVelocity, float endScale, Vector3 gravity, float lifetime, Texture2D texture, int capacity, BaseParticleInitialiser[] initialisers)
        {
            _initialisers = initialisers;
            Description = new ParticleSystemDescription
            {
                BlendState = blend,
                EndLinearVelocity = endLinearVelocity,
                EndScale = endScale,
                Gravity = gravity,
                Lifetime = lifetime,
                Texture = texture,
                Capacity = capacity
            };
        }

        public void Setup(ParticleEmitter emitter)
        {
            emitter.Description = Description;

            foreach (var initialiser in _initialisers)
                emitter.AddInitialiser((BaseParticleInitialiser)initialiser.Clone());
        }
    }

    public class ParticleSystemGeneratorReader
        :ContentTypeReader<ParticleSystemGenerator>
    {
        protected override ParticleSystemGenerator Read(ContentReader input, ParticleSystemGenerator existingInstance)
        {
            return new ParticleSystemGenerator(
                input.ReadObject<BlendState>(),
                input.ReadSingle(),
                input.ReadSingle(),
                input.ReadVector3(),
                input.ReadSingle(),
                input.ContentManager.Load<Texture2D>(input.ReadString()),
                input.ReadInt32(),
                ReadInitialisers(input)
            );
        }

        private BaseParticleInitialiser[] ReadInitialisers(ContentReader input)
        {
            int length = input.ReadInt32();
            BaseParticleInitialiser[] initialisers = new BaseParticleInitialiser[length];
            for (int i = 0; i < length; i++)
                initialisers[i] = input.ReadObject<BaseParticleInitialiser>();

            return initialisers;
        }
    }
}
