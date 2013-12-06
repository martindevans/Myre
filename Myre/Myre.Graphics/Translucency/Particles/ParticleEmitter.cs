using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Myre.Collections;
using Myre.Entities;
using Myre.Entities.Behaviours;
using Myre.Entities.Services;
using Myre.Graphics.Geometry;
using Myre.Graphics.Translucency.Particles.Initialisers;
using Myre.Graphics.Translucency.Particles.Triggers;
using Ninject;

namespace Myre.Graphics.Translucency.Particles
{
    [DefaultManager(typeof(Manager))]
    public class ParticleEmitter
        : ProcessBehaviour
    {
        private readonly IKernel _kernel;
        private bool _dirty;
        private Random _random = new Random();

        private ParticleSystemDescription _description;

        public ParticleSystemDescription Description
        {
            get { return _description; }
            set
            {
                _description = value;
                _dirty = true;
            }
        }

        public bool Enabled { get; set; }

        private int _extraCapacity;
        public int Capacity
        {
            get { return _description.Capacity; }
            set { _extraCapacity = Math.Max(0, value - _description.Capacity); _dirty = true; }
        }

        public Random Random
        {
            get { return _random; }
            set { _random = value; }
        }

        public ParticleType Type
        {
            get { return _description.Type; }
            set
            {
                _description.Type = value;
                _dirty = true;
            }
        }

        public Texture2D Texture
        {
            get { return _description.Texture; }
            set
            {
                _description.Texture = value;
                _dirty = true;
            }
        }

        public BlendState BlendState
        {
            get { return _description.BlendState; }
            set
            {
                _description.BlendState = value;
                _dirty = true;
            }
        }

        public float Lifetime
        {
            get { return _description.Lifetime; }
            set
            {
                _description.Lifetime = value;
                _dirty = true;
            }
        }

        public float EndLinearVelocity
        {
            get { return _description.EndLinearVelocity; }
            set
            {
                _description.EndLinearVelocity = value;
                _dirty = true;
            }
        }
        
        public float EndScale
        {
            get { return _description.EndScale; }
            set
            {
                _description.EndScale = value;
                _dirty = true;
            }
        }

        public Vector3 Gravity
        {
            get { return _description.Gravity; }
            set
            {
                _description.Gravity = value;
                _dirty = true;
            }
        }

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

        private bool Dirty
        {
            get { return _dirty; }
            set { _dirty = value; }
        }

        public ParticleEmitter(IKernel kernel)
        {
            _kernel = kernel;
            _dirty = true;
        }

        public override void Initialise(INamedDataProvider initialisationData)
        {
            initialisationData.GetValue<ParticleSystemGenerator>("particlesystem" + AppendName(), false).Setup(this);

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
            if (Dirty)
            {
                if (System == null)
                    CreateParticleSystem();
                else
                {
                    System.GrowCapacity(_extraCapacity);
                    _extraCapacity = 0;
                }
                Dirty = false;
            }

            if (Enabled)
                System.Update(dt);

            foreach (var trigger in Triggers)
                trigger.Update(dt);

            foreach (var baseParticleInitialiser in _initialisers)
                baseParticleInitialiser.Update(dt);
        }

        protected void CreateParticleSystem()
        {
            System = _kernel.Get<ParticleSystem>();
            System.Initialise(_description);
        }

        protected virtual void Draw(NamedBoxCollection metadata)
        {
            if (System != null)
                System.Draw(metadata);
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
            public override void Initialise(Scene scene)
            {
                var processes = scene.GetService<ProcessService>();
                processes.Add(this);

                base.Initialise(scene);
            }

            public void Draw(string phase, NamedBoxCollection metadata)
            {
                if (phase == "translucent")
                    foreach (var particleEmitter in Behaviours)
                        particleEmitter.Draw(metadata);
            }
        }
    }
}
