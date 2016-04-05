using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Threading.Tasks;
using Myre.Entities.Services;

namespace Myre.Entities.Behaviours
{
    /// <summary>
    /// A behaviour which updates in parallel with all other instances of itself.
    /// This simplifies implementation significantly as the only threading hazards are with yourself, not the rest of the entire game/engine!
    /// </summary>
    public abstract class ParallelProcessBehaviour
        : Behaviour
    {
        protected ParallelProcessBehaviour(string name)
            : base(name)
        {
        }

        protected ParallelProcessBehaviour()
            : base(null)
        {
        }

        protected abstract void ParallelUpdate(float elapsedTime);

        public class Manager<B>
            : BehaviourManager<B>, IProcess
            where B : ParallelProcessBehaviour
        {
            public bool IsComplete
            {
                get
                {
                    Contract.Ensures(!Contract.Result<bool>());
                    return false;
                }
            }

            public override void Initialise(Scene scene)
            {
                scene.GetService<ProcessService>().Add(this);
                base.Initialise(scene);
            }

            void IProcess.Update(float elapsedTime)
            {
                Update(elapsedTime);
            }

            private readonly List<B> _toAdd = new List<B>();

            public override void Add(B behaviour)
            {
                _toAdd.Add(behaviour);
            }

            private readonly List<B> _toRemove = new List<B>();

            public override bool Remove(B behaviour)
            {
                if (Behaviours.Contains(behaviour))
                {
                    _toRemove.Add(behaviour);
                    return true;
                }
                return false;
            }

            private float _latestElapsedTime;
            protected virtual void Update(float elapsedTime)
            {
                _latestElapsedTime = elapsedTime;

                foreach (var behaviour in _toAdd)
                    base.Add(behaviour);
                _toAdd.Clear();

                foreach (var behaviour in _toRemove)
                    base.Remove(behaviour);
                _toRemove.Clear();

#if DEBUG
                foreach (var behaviour in Behaviours)
                    InvokeUpdate(behaviour);
#else
                Parallel.ForEach(Behaviours, InvokeUpdate);
#endif
            }

            private void InvokeUpdate(ParallelProcessBehaviour behaviour)
            {
                Contract.Requires(behaviour != null);

                behaviour.ParallelUpdate(_latestElapsedTime);
            }
        }
    }
}
