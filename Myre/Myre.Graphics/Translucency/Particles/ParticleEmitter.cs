using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Myre.Collections;
using Myre.Entities;
using Myre.Entities.Behaviours;
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
            var generator = initialisationData.GetValue(new TypedName<ParticleEmitterDescription>("particlesystem"), false);
            generator.Setup(this);

            System = Owner.Scene.GetService<ParticleSystemService>().Get(generator.Description);
            UpdateRadiusEstimate();

            base.Initialise(initialisationData);
        }

        private void UpdateRadiusEstimate()
        {
            //Update the bounding sphere estimate for the system
            Particle p = new Particle();
            foreach (var baseParticleInitialiser in Initialisers)
                baseParticleInitialiser.Maximise(ref p);

            var lifetime = p.LifetimeScale * System.Description.Lifetime;

            //We estimate distance travelled. However this calculation does not seem to be in world space coordinates - it produces estimates far too big!
            //todo: incorporate particle velocity into radius estimate
            var distanceTravelled = p.Velocity * lifetime + (System.Description.EndLinearVelocity * p.Velocity - p.Velocity) * lifetime * lifetime * 0.5f
                                    + System.Description.Gravity * 0.5f * lifetime * lifetime;


            System.RadiusEstimate = p.Position.Length();
        }

        public void AddInitialiser(BaseParticleInitialiser initialiser)
        {
            initialiser.Attach(this);
            _initialisers.Add(initialiser);

            if (System != null)
                UpdateRadiusEstimate();
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

            public void Query(string phase, NamedBoxCollection metadata, ICollection<IGeometry> result)
            {
                if (phase != "translucent")
                    return;

                _systems.Query(phase, metadata, result);
            }
        }
    }
}
