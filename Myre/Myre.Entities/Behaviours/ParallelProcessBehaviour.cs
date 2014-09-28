using System.Collections.Generic;
using System.Threading.Tasks;
using Myre.Entities.Services;

namespace Myre.Entities.Behaviours
{
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
            public new IEnumerable<B> Behaviours
            {
                get { return base.Behaviours; }
            }

            public bool IsComplete
            {
                get { return false; }
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
                if (base.Behaviours.Contains(behaviour))
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

                for (int i = 0; i < _toAdd.Count; i++)
                    base.Add(_toAdd[i]);
                _toAdd.Clear();

                for (int i = 0; i < _toRemove.Count; i++)
                    base.Remove(_toRemove[i]);
                _toRemove.Clear();

                Parallel.ForEach(Behaviours, InvokeUpdate);
            }

            private void InvokeUpdate(ParallelProcessBehaviour behaviour)
            {
                behaviour.ParallelUpdate(_latestElapsedTime);
            }
        }
    }
}
