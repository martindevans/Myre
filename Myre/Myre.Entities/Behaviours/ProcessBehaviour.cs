using System.Collections.Generic;
using Myre.Entities.Services;

namespace Myre.Entities.Behaviours
{
    public abstract class ProcessBehaviour
        : Behaviour
    {
        protected abstract void Update(float elapsedTime);

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

            protected virtual void Update(float elapsedTime)
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
