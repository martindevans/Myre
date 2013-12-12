using System;
using System.Collections.Generic;
using Myre.Collections;
using Myre.Entities;
using Myre.Entities.Behaviours;
using Myre.Entities.Services;
using Myre.Graphics.Geometry;
using Myre.Graphics.Translucency.Particles.Initialisers;
using Myre.Graphics.Translucency.Particles.Triggers;

namespace Myre.Graphics.Translucency.Particles
{
    [DefaultManager(typeof(Manager))]
    public class ParticleEmitter
        : ProcessBehaviour
    {
        private readonly Random _random = new Random();

        private readonly List<BaseParticleInitialiser> _initialisers = new List<BaseParticleInitialiser>();

        public IEnumerable<BaseParticleInitialiser> Initialisers
        {
            get { return _initialisers; }
        }

        private readonly List<ITrigger> _triggers = new List<ITrigger>();

        public IEnumerable<ITrigger> Triggers
        {
            get { return _triggers; }
        }

        private ParticleSystem System { get; set; }

        public override void Initialise(INamedDataProvider initialisationData)
        {
            var generator = initialisationData.GetValue<ParticleEmitterDescription>("particlesystem" + AppendName(), false);
            generator.Setup(this);
            System = Owner.Scene.GetService<ParticleSystemService>().Get(generator.Description);

            base.Initialise(initialisationData);
        }

        public void AddInitialiser(BaseParticleInitialiser initialiser)
        {
            initialiser.Attach(this);
            _initialisers.Add(initialiser);
        }

        public void AddTrigger(ITrigger trigger)
        {
            trigger.Attach(this);
            _triggers.Add(trigger);
        }

        /// <summary>
        /// Reset all triggers
        /// </summary>
        public void Reset()
        {
            foreach (var trigger in Triggers)
                trigger.Reset();
        }

        protected override void Update(float dt)
        {
            if (System != null)
                System.Update(dt);

            foreach (var trigger in Triggers)
                trigger.Update(dt);

            foreach (var baseParticleInitialiser in _initialisers)
                baseParticleInitialiser.Update(dt);
        }

        public void Spawn(Particle particle)
        {
            foreach (var initialiser in _initialisers)
                initialiser.Initialise(_random, ref particle);

            System.Spawn(particle.Position, particle.Velocity, particle.AngularVelocity, particle.Size, particle.LifetimeScale, particle.StartColour, particle.EndColour);
        }

        public class Manager
            : Manager<ParticleEmitter>, IGeometryProvider
        {
            private ParticleSystemService _systems;

            public override void Initialise(Scene scene)
            {
                _systems = scene.GetService<ParticleSystemService>();

                base.Initialise(scene);
            }

            public void Draw(string phase, NamedBoxCollection metadata)
            {
                _systems.Draw(phase, metadata);
            }
        }
    }
}
