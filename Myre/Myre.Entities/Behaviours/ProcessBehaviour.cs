using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Threading;
using Myre.Entities.Services;

namespace Myre.Entities.Behaviours
{
    public abstract class ProcessBehaviour
        : Behaviour
    {
        protected abstract void Update(float elapsedTime);

        /// <summary>
        /// How often this behaviour updates. A Period of 0 means every frame
        /// </summary>
        protected uint Period { get; set; }

        private static int _nextCounter;
        private uint _counter;

        protected ProcessBehaviour()
        {
            Period = 0;
            _counter = new IntUIntUnion { IntValue = Interlocked.Increment(ref _nextCounter) }.UIntValue;   //Spread updates out across time to prevent clumping
        }

        public class Manager<TB>
            : BehaviourManager<TB>, IProcess
            where TB : ProcessBehaviour
        {
            public new IEnumerable<TB> Behaviours
            {
                get
                {
                    Contract.Ensures(Contract.Result<IEnumerable<TB>>() != null);
                    return base.Behaviours;
                }
            }

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
                if (base.Behaviours.Contains(behaviour))
                {
                    _toRemove.Add(behaviour);
                    return true;
                }
                return false;
            }

            protected virtual void Update(float elapsedTime)
            {
                foreach (TB behaviour in _toAdd)
                    base.Add(behaviour);
                _toAdd.Clear();

                foreach (TB behaviour in _toRemove)
                    base.Remove(behaviour);
                _toRemove.Clear();

                foreach (var item in Behaviours)
                {
                    if (!item.Owner.IsDisposed)
                    {
                        unchecked { item._counter++; }  //Don't really care if this overflows
                        if (item.Period == 0 || (item.Period != uint.MaxValue && item._counter % (item.Period + 1) == item.Period))
                            item.Update(elapsedTime);
                    }
                }
            }
        }
    }
}
