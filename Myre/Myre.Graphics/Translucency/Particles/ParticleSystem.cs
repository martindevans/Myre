// Based upon the Particles3D sample on AppHub.
// 

using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Graphics.PackedVector;
using Myre.Collections;
using Myre.Graphics.Materials;

namespace Myre.Graphics.Translucency.Particles
{
    /// <summary>
    /// A class which manages and updating and rendering of particles.
    /// </summary>
    public class ParticleSystem
    {
        private readonly GraphicsDevice _device;
        private readonly Material _material;
        private EffectParameter _currentTimeParameter;
        private EffectParameter _viewportScaleParameter;

        private ParticleVertex[] _particles;
        private DynamicVertexBuffer _vertices;
        private IndexBuffer _indices;

        private int _active;
        private int _newlyCreated;
        private int _free;
        private int _finished;

        private float _time;
        private int _frameCounter;

        private bool _dirty;

        /// <summary>
        /// Gets the settings for this particle system.
        /// </summary>
        public ParticleSystemDescription Description { get; private set; }

        /// <summary>
        /// Gets or sets the world transformation matrix to apply to this <see cref="ParticleSystem"/>s' particles.
        /// </summary>
        public Matrix Transform { get; set; }

        /// <summary>
        /// Gets the number of active particles.
        /// </summary>
        public int ActiveCount
        {
            get
            {
                if (_active < _newlyCreated)
                    return _newlyCreated - _active;

                return _newlyCreated + (Description.Capacity - _active);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ParticleSystem"/> class.
        /// </summary>
        /// <param name="device">The device.</param>
        public ParticleSystem(GraphicsDevice device)
        {
            _device = device;
            _material = new Material(Content.Load<Effect>("ParticleSystem").Clone());
            Transform = Matrix.Identity;
        }

        /// <summary>
        /// Initialises this <see cref="ParticleSystem"/> instance.
        /// </summary>
        /// <param name="description"></param>
        public void Initialise(ParticleSystemDescription description)
        {
            Description = description;

            _material.Parameters["Texture"].SetValue(description.Texture);
            _material.Parameters["Lifetime"].SetValue(description.Lifetime);
            _material.Parameters["EndVelocity"].SetValue(description.EndLinearVelocity);
            _material.Parameters["EndScale"].SetValue(description.EndScale);
            _material.Parameters["Gravity"].SetValue(description.Gravity);

            _currentTimeParameter = _material.Parameters["Time"];
            _viewportScaleParameter = _material.Parameters["ViewportScale"];

            InitialiseBuffer();
        }

        /// <summary>
        /// Increases the capacity of this system by the specified amount.
        /// </summary>
        /// <param name="size">The amount by which to increase the capacity.</param>
        public void GrowCapacity(int size)
        {
            Description = new ParticleSystemDescription
            {
                BlendState = Description.BlendState,
                EndLinearVelocity = Description.EndLinearVelocity,
                EndScale = Description.EndScale,
                Gravity = Description.Gravity,
                Lifetime = Description.Lifetime,
                Texture = Description.Texture,
                Type = Description.Type,
                Capacity = Description.Capacity + size,
            };
            _dirty = true;
        }

        /// <summary>
        /// Spawns a new particle.
        /// </summary>
        /// <param name="position">The position.</param>
        /// <param name="velocity">The velocity.</param>
        /// <param name="angularVelocity">The angular velocity.</param>
        /// <param name="size">The size.</param>
        /// <param name="lifetimeScale">The lifetime scale. This should be between 0 and 1.</param>
        /// <param name="startColour">The start colour.</param>
        /// <param name="endColour">The end colour.</param>
        public void Spawn(Vector3 position, Vector3 velocity, float angularVelocity, float size, float lifetimeScale, Color startColour, Color endColour)
        {
            if (_dirty)
                InitialiseBuffer();

            // exit if we have run out of capacity
            int nextFreeParticle = (_free + 1) % Description.Capacity;
            if (nextFreeParticle == _finished)
                return;

            // write data into buffer
            for (int i = 0; i < 4; i++)
            {
                _particles[_free * 4 + i].Position = position;
                _particles[_free * 4 + i].Velocity = new Vector4(velocity, angularVelocity);
                _particles[_free * 4 + i].Scales = new HalfVector2(size, lifetimeScale > 0 ? 1f / lifetimeScale : 1);
                _particles[_free * 4 + i].StartColour = startColour;
                _particles[_free * 4 + i].EndColour = endColour;
                _particles[_free * 4 + i].Time = _time;
            }

            _free = nextFreeParticle;
        }

        public void Update(float dt)
        {
            if (_dirty)
                InitialiseBuffer();

            _time += dt;

            RetireActiveParticles();
            FreeRetiredParticles();

            bool noActiveParticles = _active == _free;
            bool noFinishedParticles = _finished == _active;

            if (noActiveParticles && noFinishedParticles)
            {
                _time = 0;
                _frameCounter = 0;
            }
        }

        public void Draw(NamedBoxCollection data)
        {
            Debug.Assert(Description.Texture != null, "Particle systems must be initialised before they can be drawn.");
            //Debug.WriteLine(string.Format("retired: {0}, active: {1}, new: {2}, free: {3}", finished, active, newlyCreated, free));

            if (_dirty)
                InitialiseBuffer();

            if (_newlyCreated != _free)
                AddNewParticlesToVertexBuffer();

            if (_vertices.IsContentLost)
                _vertices.SetData(_particles);

            // If there are any active particles, draw them now!
            if (_active != _free)
            {
                _device.BlendState = Description.BlendState;
                _device.DepthStencilState = DepthStencilState.DepthRead;

                // Set an effect parameter describing the viewport size. This is
                // needed to convert particle sizes into screen space point sizes.
                _viewportScaleParameter.SetValue(new Vector2(0.5f / data.GetValue(new TypedName<Viewport>("viewport")).AspectRatio, -0.5f));

                // Set an effect parameter describing the current time. All the vertex
                // shader particle animation is keyed off this value.
                _currentTimeParameter.SetValue(_time);

                data.Set<Matrix>("world", Transform);

                // Set the particle vertex and index buffer.
                _device.SetVertexBuffer(_vertices);
                _device.Indices = _indices;

                SelectParticleType();
                
                // Activate the particle effect.
                foreach (EffectPass pass in _material.Begin(data))
                {
                    pass.Apply();

                    // work around for an xna 4.0 bug
                    _device.SamplerStates[0] = SamplerState.PointClamp;
                    _device.SamplerStates[1] = SamplerState.PointClamp;
                    _device.SamplerStates[2] = SamplerState.PointClamp;

                    if (_active < _free)
                    {
                        // If the active particles are all in one consecutive range,
                        // we can draw them all in a single call.
                        _device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0,
                                                     _active * 4, (_free - _active) * 4,
                                                     _active * 6, (_free - _active) * 2);
                    }
                    else
                    {
                        // If the active particle range wraps past the end of the queue
                        // back to the start, we must split them over two draw calls.
                        _device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0,
                                                     _active * 4, (Description.Capacity - _active) * 4,
                                                     _active * 6, (Description.Capacity - _active) * 2);

                        if (_free > 0)
                        {
                            _device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0,
                                                         0, _free * 4,
                                                         0, _free * 2);
                        }
                    }
                }

                // Reset some of the renderstates that we changed,
                // so as not to mess up any other subsequent drawing.
                _device.DepthStencilState = DepthStencilState.Default;
                _device.BlendState = BlendState.AlphaBlend;
            }

            _frameCounter++;
        }

        private void SelectParticleType()
        {
            switch (Description.Type)
            {
                case ParticleType.Hard:
                    _material.CurrentTechnique = _material.Techniques["Hard"];
                    break;
                case ParticleType.Soft:
                    _material.CurrentTechnique = _material.Techniques["Soft"];
                    break;
            }
        }

        private void InitialiseBuffer()
        {
            // dispose exiting buffers
            if (_vertices != null)
                _vertices.Dispose();
            if (_indices != null)
                _indices.Dispose();

            // create new vertex buffer
            _vertices = new DynamicVertexBuffer(_device, ParticleVertex.VertexDeclaration, Description.Capacity * 4, BufferUsage.WriteOnly);
            
            // set up quad corners
            var particles = new ParticleVertex[Description.Capacity * 4];
            for (int i = 0; i < Description.Capacity; i++)
            {
                particles[i * 4 + 0].Corner = new Short2(-1, -1);
                particles[i * 4 + 1].Corner = new Short2(1, -1);
                particles[i * 4 + 2].Corner = new Short2(1, 1);
                particles[i * 4 + 3].Corner = new Short2(-1, 1);
            }

            // copy over any exiting active particles
            int j = 0;
            for (int i = _active *4; i != _free * 4; i = (i + 1) % particles.Length)
            {
                particles[j] = _particles[i];
                j++;
            }

            // swap array over to the new larger array
            _particles = particles;

            // create new index buffer
            ushort[] indices = new ushort[Description.Capacity * 6];
            for (int i = 0; i < Description.Capacity; i++)
            {
                indices[i * 6 + 0] = (ushort)(i * 4 + 0);
                indices[i * 6 + 1] = (ushort)(i * 4 + 1);
                indices[i * 6 + 2] = (ushort)(i * 4 + 2);

                indices[i * 6 + 3] = (ushort)(i * 4 + 0);
                indices[i * 6 + 4] = (ushort)(i * 4 + 2);
                indices[i * 6 + 5] = (ushort)(i * 4 + 3);
            }

            _indices = new IndexBuffer(_device, typeof(ushort), indices.Length, BufferUsage.WriteOnly);
            _indices.SetData(indices);

            _dirty = false;
        }

        /// <summary>
        /// Helper for checking when active particles have reached the end of
        /// their life. It moves old particles from the active area of the queue
        /// to the retired section.
        /// </summary>
        // Modified from Particles3D sample
        private void RetireActiveParticles()
        {
            while (_active != _free)
            {
                // Is this particle old enough to retire?
                // We multiply the active particle index by four, because each
                // particle consists of a quad that is made up of four vertices.
                float particleAge = _time - _particles[_active * 4].Time;

                if (particleAge < Description.Lifetime)
                    break;

                // Remember the time at which we retired this particle.
                _particles[_active * 4].Time = _frameCounter;

                // Move the particle from the active to the retired queue.
                _active = (_active + 1) % Description.Capacity;
            }
        }


        /// <summary>
        /// Helper for checking when retired particles have been kept around long
        /// enough that we can be sure the GPU is no longer using them. It moves
        /// old particles from the retired area of the queue to the free section.
        /// </summary>
        // Modified from Particles3D sample
        private void FreeRetiredParticles()
        {
            while (_finished != _active)
            {
                // Has this particle been unused long enough that
                // the GPU is sure to be finished with it?
                // We multiply the retired particle index by four, because each
                // particle consists of a quad that is made up of four vertices.
                int age = _frameCounter - (int)_particles[_finished * 4].Time;

                // The GPU is never supposed to get more than 2 frames behind the CPU.
                // We add 1 to that, just to be safe in case of buggy drivers that
                // might bend the rules and let the GPU get further behind.
                if (age < 3)
                    break;

                // Move the particle from the retired to the free queue.
                _finished = (_finished + 1) % Description.Capacity;
            }
        }

        /// <summary>
        /// Helper for uploading new particles from our managed
        /// array to the GPU vertex buffer.
        /// </summary>
        // Modified from Particles3D sample
        void AddNewParticlesToVertexBuffer()
        {
            const int stride = ParticleVertex.SIZE_IN_BYTES;

            if (_newlyCreated < _free)
            {
                // If the new particles are all in one consecutive range,
                // we can upload them all in a single call.
                _vertices.SetData(_newlyCreated * stride * 4, _particles,
                                 _newlyCreated * 4, (_free - _newlyCreated) * 4,
                                 stride, SetDataOptions.NoOverwrite);
            }
            else
            {
                // If the new particle range wraps past the end of the queue
                // back to the start, we must split them over two upload calls.
                _vertices.SetData(_newlyCreated * stride * 4, _particles,
                                 _newlyCreated * 4,
                                 (Description.Capacity - _newlyCreated) * 4,
                                 stride, SetDataOptions.NoOverwrite);

                if (_free > 0)
                {
                    _vertices.SetData(0, _particles,
                                     0, _free * 4,
                                     stride, SetDataOptions.NoOverwrite);
                }
            }

            // Move the particles we just uploaded from the new to the active queue.
            _newlyCreated = _free;
        }
    }
}
