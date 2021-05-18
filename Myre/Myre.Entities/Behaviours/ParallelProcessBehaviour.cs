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
        protected abstract void ParallelUpdate(float elapsedTime);

        public class Manager<TB>
            : BehaviourManager<TB>, IProcess
            where TB : ParallelProcessBehaviour
        {
            /// <summary>
            /// If the number of behaviours to update is less than this the update will be done serially
            /// </summary>
            public virtual uint ParallelThreshold => 128;

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

            private readonly List<TB> _toAdd = new();

            public override void Add(TB behaviour)
            {
                _toAdd.Add(behaviour);
            }

            private readonly List<TB> _toRemove = new();

            public override bool Remove(TB behaviour)
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

                if (Behaviours.Count < ParallelThreshold)
                {
                    foreach (var item in Behaviours)
                        item.ParallelUpdate(elapsedTime);
                }
                else
                    Parallel.ForEach(Behaviours, InvokeUpdate);
            }

            private void InvokeUpdate(ParallelProcessBehaviour behaviour)
            {
                behaviour.ParallelUpdate(_latestElapsedTime);
            }
        }
    }
}
