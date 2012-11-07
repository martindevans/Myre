using System.Collections.Generic;
using Myre.Entities;
using Myre.Entities.Behaviours;
using Myre.Entities.Services;

namespace Myre.Entities.Behaviours
{
    public abstract class ProcessBehaviour
        : Behaviour
    {
        protected abstract void Update(float elapsedTime);

        public class Manager<B>
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
                foreach (var item in Behaviours)
                {
                    if (item.Owner != null && !item.Owner.IsDisposed)
                        item.Update(elapsedTime);
                }
            }
        }
    }
}
