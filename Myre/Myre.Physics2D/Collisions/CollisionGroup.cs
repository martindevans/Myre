using System.Collections.Generic;

namespace Myre.Physics2D.Collisions
{
    public class CollisionGroup
    {
        private readonly HashSet<CollisionGroup> _ignores = new HashSet<CollisionGroup>();

        public void Ignore(CollisionGroup group)
        {
            _ignores.Add(group);
        }

        public bool Ignores(CollisionGroup group)
        {
            return IgnoresNonRecursive(group) || group.IgnoresNonRecursive(this);
        }

        private bool IgnoresNonRecursive(CollisionGroup group)
        {
            return _ignores.Contains(group);
        }
    }
}
