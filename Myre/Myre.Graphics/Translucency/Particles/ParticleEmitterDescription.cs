using Microsoft.Xna.Framework.Content;
using Myre.Graphics.Translucency.Particles.Initialisers;
using Myre.Graphics.Translucency.Particles.Triggers;

namespace Myre.Graphics.Translucency.Particles
{
    public class ParticleEmitterDescription
    {
        private readonly ITrigger[] _triggers;
        private readonly BaseParticleInitialiser[] _initialisers;
        public ParticleSystemDescription Description;

        public ParticleEmitterDescription(ParticleSystemDescription description, ITrigger[] triggers, BaseParticleInitialiser[] initialisers)
        {
            _triggers = triggers;
            _initialisers = initialisers;

            Description = description;
        }

        public void Setup(ParticleEmitter emitter)
        {
            foreach (var trigger in _triggers)
                emitter.AddTrigger((ITrigger)trigger.Clone());

            foreach (var initialiser in _initialisers)
                emitter.AddInitialiser((BaseParticleInitialiser)initialiser.Clone());
        }
    }

    public class ParticleEmitterDescriptionReader
        :ContentTypeReader<ParticleEmitterDescription>
    {
        protected override ParticleEmitterDescription Read(ContentReader input, ParticleEmitterDescription existingInstance)
        {
            return new ParticleEmitterDescription(
                input.ContentManager.Load<ParticleSystemDescription>(input.ReadString()),
                ReadTriggers(input),
                ReadInitialisers(input)
            );
        }

        private ITrigger[] ReadTriggers(ContentReader input)
        {
            int length = input.ReadInt32();
            ITrigger[] triggers = new ITrigger[length];
            for (int i = 0; i < length; i++)
                triggers[i] = input.ReadObject<ITrigger>();

            return triggers;
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
