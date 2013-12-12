using Myre.Graphics.Pipeline.Particles.Initialisers;
using Myre.Graphics.Pipeline.Particles.Triggers;

namespace Myre.Graphics.Pipeline.Particles
{
    /// <summary>
    /// Describes an emitter which adds particles to a system
    /// </summary>
    public class ParticleEmitter
    {
        /// <summary>
        /// The system which this emitter adds particles to
        /// </summary>
        public string System;

        /// <summary>
        /// Things which trigger new particles to be created
        /// </summary>
        public ITrigger[] Triggers;

        /// <summary>
        /// Things which set up the initial state of new particles when they're created
        /// </summary>
        public IInitialiser[] Initialisers;
    }
}
