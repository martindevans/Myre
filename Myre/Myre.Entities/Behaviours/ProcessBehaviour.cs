using System.Collections.Generic;
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

// ReSharper disable MemberCanBeProtected.Global
// ReSharper disable ClassWithVirtualMembersNeverInherited.Global
        public class Manager<B>
// ReSharper restore ClassWithVirtualMembersNeverInherited.Global
// ReSharper restore MemberCanBeProtected.Global
            : BehaviourManager<B>, IProcess
            where B : ProcessBehaviour
        {
            public new IEnumerable<B> Behaviours
            {
                get
                {
                    return base.Behaviours;
                }
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

            protected virtual void Update(float elapsedTime)
            {
                for (int i = 0; i < _toAdd.Count; i++)
                    base.Add(_toAdd[i]);
                _toAdd.Clear();

                for (int i = 0; i < _toRemove.Count; i++)
                    base.Remove(_toRemove[i]);
                _toRemove.Clear();

                foreach (var item in Behaviours)
                {
                    if (item.Owner != null && !item.Owner.IsDisposed)
                    {
                        unchecked { item._counter++; }  //Don't really care if this overflows
                        if (item._counter % (item.Period + 1) == item.Period)
                            item.Update(elapsedTime);
                    }
                }
            }
        }
    }
}
