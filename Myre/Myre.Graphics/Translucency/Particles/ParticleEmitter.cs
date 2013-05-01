using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Myre.Collections;
using Myre.Entities;
using Myre.Entities.Behaviours;
using Myre.Entities.Services;
using Myre.Graphics.Geometry;
using Ninject;

namespace Myre.Graphics.Translucency.Particles
{
    [DefaultManager(typeof(Manager))]
    public abstract class ParticleEmitter
        : Behaviour
    {
        private static readonly List<ParticleSystem> _systems = new List<ParticleSystem>();

        private readonly IKernel _kernel;
        private ParticleSystem _system;
        private ParticleSystemDescription _description;
        private bool _dirty;

        public bool Enabled { get; set; }

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

        protected ParticleSystem System
        {
            get { return _system; }
        }

        protected bool Dirty
        {
            get { return _dirty; }
            set { _dirty = value; }
        }

        protected bool UsingUniqueSystem { get; set; }

        public ParticleEmitter(IKernel kernel)
        {
            _kernel = kernel;
            _dirty = true;
        }

        protected abstract void Update(float dt);

        protected void CreateParticleSystem()//bool unique)
        {
            //if (unique)
            //{
                _system = _kernel.Get<ParticleSystem>();
                _system.Initialise(_description);
                _systems.Add(_system);
            //}
            //else
            //{
            //    if (!systemsDictionary.TryGetValue(description, out system))
            //    {
            //        system = kernel.Get<ParticleSystem>();
            //        system.Initialise(description);

            //        systems.Add(system);
            //        systemsDictionary[description] = system;
            //    }
            //}

            //UsingUniqueSystem = unique;
        }

        public class Manager
            : BehaviourManager<ParticleEmitter>, IProcess, IGeometryProvider
        {
            public bool IsComplete { get { return false; } }

            public override void Initialise(Scene scene)
            {
                var processes = scene.GetService<ProcessService>();
                processes.Add(this);

                base.Initialise(scene);
            }

            public void Update(float elapsedTime)
            {
                foreach (var item in Behaviours)
                {
                    if (item.Enabled)
                        item.Update(elapsedTime);
                }

                foreach (var item in _systems)
                {
                    item.Update(elapsedTime);
                }
            }

            public void Draw(Renderer renderer)
            {
                foreach (var item in _systems)
                {
                    item.Draw(renderer.Data);
                }
            }

            public void Draw(string phase, BoxedValueStore<string> metadata)
            {
                if (phase == "translucent")
                    foreach (var particleSystem in _systems)
                        particleSystem.Draw(metadata);
            }
        }
    }
}
