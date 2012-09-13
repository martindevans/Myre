using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Myre.Physics2.Collisions
{
    public class CollisionDetector
    {
        private readonly List<Collision> _collisions;
        private readonly ReadOnlyCollection<Collision> _collisionsWrapper;
        private readonly List<Geometry> _geometry;
        private readonly SatTester _tester;

        public ReadOnlyCollection<Collision> Collisions
        {
            get { return _collisionsWrapper; }
        }

        public CollisionDetector()
        {
            _collisions = new List<Collision>();
            _collisionsWrapper = new ReadOnlyCollection<Collision>(_collisions);
            _geometry = new List<Geometry>();
            _tester = new SatTester();
        }

        public void Add(Geometry geom)
        {
            _geometry.Add(geom);
        }

        public bool Remove(Geometry geom)
        {
            if (_geometry.Remove(geom))
            {
                for (int i = _collisions.Count - 1; i >= 0; i--)
                {
                    var collision = _collisions[i];
                    if (collision.GeometryA == geom || collision.GeometryB == geom)
                    {
                        collision.Dispose();
                        _collisions.RemoveAt(i);
                    }
                }

                return true;
            }

            return false;
        }

        public void Update()
        {
            DoBroadphase();
            DoNarrowphase();
            RemoveDeadCollisions();
        }

        private void DoBroadphase()
        {
            // TODO: Implement Sweep and prune ;)
            for (int i = 0; i < _geometry.Count; i++)
            {
                for (int j = i + 1; j < _geometry.Count; j++)
                {
                    var a = _geometry[i];
                    var b = _geometry[j];

                    if (float.IsPositiveInfinity(a.Body.Mass) && float.IsPositiveInfinity(b.Body.Mass)
                        && float.IsPositiveInfinity(a.Body.InertiaTensor) && float.IsPositiveInfinity(b.Body.InertiaTensor))
                        continue;

                    if (a.Body.Sleeping && b.Body.Sleeping)
                        continue;

                    if (a.Bounds.Intersects(b.Bounds))
                    {
                        if (!a.collidingWith.Contains(b))
                        {
                            var collision = Collision.Create(_geometry[i], _geometry[j]);
                            _collisions.Add(collision);
                        }
                    }
                }
            }
        }

        private void DoNarrowphase()
        {
            for (int i = 0; i < _collisions.Count; i++)
            {
                var collision = _collisions[i];
                collision.FindContacts(_tester);

                if (collision.Contacts.Count > 0)
                {
                    if (!collision.GeometryB.Body.IsStatic && !collision.GeometryB.Body.Sleeping)
                        collision.GeometryA.Body.Sleeping = false;

                    if (!collision.GeometryA.Body.IsStatic && !collision.GeometryA.Body.Sleeping)
                        collision.GeometryB.Body.Sleeping = false;
                }

                //DynamicPhysics activatedBody;
                //if (collisions[i].ShouldActivateBody(out activatedBody))
                //{
                //    activatedBody.Sleeping = false;

                //    for (int j = i - 1; j >= 0; j--)
                //    {
                //        if (collisions[j].A.Body == activatedBody || collisions[j].B.Body == activatedBody)
                //            collisions[j].FindContacts(tester);
                //    }
                //}
            }
        }

        private void RemoveDeadCollisions()
        {
            for (int i = _collisions.Count - 1; i >= 0; i--)
            {
                if (_collisions[i].IsDead)
                {
                    _collisions[i].Dispose();
                    _collisions.RemoveAt(i);
                }
            }
        }
    }
}
