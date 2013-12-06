
namespace Myre.Graphics.Translucency.Particles.Triggers
{
    public interface ITrigger : ICopyable
    {
        /// <summary>
        /// Attach this trigger to an emitter
        /// </summary>
        /// <param name="emitter"></param>
        void Attach(ParticleEmitter emitter);

        /// <summary>
        /// Update this trigger, emit particles as necessary
        /// </summary>
        /// <param name="dt"></param>
        void Update(float dt);

        /// <summary>
        /// Reset this trigger
        /// </summary>
        void Reset();
    }
}
