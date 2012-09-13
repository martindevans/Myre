using System.Collections.ObjectModel;
using Myre.Entities;
using Myre.Entities.Behaviours;
using Myre.Physics2;

namespace Myre.Physics2.Collisions
{
    [DefaultManager(typeof(Manager))]
    public partial class Geometry
    {
        public class Manager
            : BehaviourManager<Geometry>, ICollisionResolver
        {
            private readonly CollisionDetector _collisionDetector;

            public ReadOnlyCollection<Collision> Collisions
            {
                get { return _collisionDetector.Collisions; }
            }

            public ReadOnlyCollection<Geometry> Geometry
            {
                get;
                private set;
            }

            public Manager(Scene scene)
            {
                _collisionDetector = new CollisionDetector();
                Geometry = new ReadOnlyCollection<Geometry>(Behaviours);
            }

            public override void Add(Geometry behaviour)
            {
                _collisionDetector.Add(behaviour);
                base.Add(behaviour);
            }

            public override bool Remove(Geometry behaviour)
            {
                _collisionDetector.Remove(behaviour);
                return base.Remove(behaviour);
            }

            public void Update(float time, float allowedPenetration, float biasFactor, int iterations)
            {
                _collisionDetector.Update();

                var inverseDt = 1f / time;
                for (int i = 0; i < _collisionDetector.Collisions.Count; i++)
                    _collisionDetector.Collisions[i].Prepare(allowedPenetration, biasFactor, inverseDt);

                for (int i = 0; i < iterations; i++)
                {
                    for (int j = 0; j < _collisionDetector.Collisions.Count; j++)
                        _collisionDetector.Collisions[j].Iterate();
                }
            }
        }
    }
}
