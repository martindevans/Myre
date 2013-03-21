using Microsoft.Xna.Framework;

namespace Myre.Physics2D.Collisions
{
    public struct Contact
    {
        public struct ContactID
        {
            public readonly Geometry Geometry;
            public readonly int Feature;

            public ContactID(Geometry geometry, int feature)
            {
                Geometry = geometry;
                Feature = feature;
            }

            public override int GetHashCode()
            {
                return Geometry.GetHashCode() ^ Feature.GetHashCode();
            }

            public override bool Equals(object obj)
            {
                if (obj is ContactID)
                    return Equals((ContactID)obj);
                else
                    return base.Equals(obj);
            }

            public bool Equals(ContactID obj)
            {
                return Geometry == obj.Geometry
                    && Feature == obj.Feature;
            }
        }

        public Vector2 Position;
        public readonly ContactID ID;
        internal float MassNormal;
        internal float MassTangent;
        internal float NormalVelocityBias;
        internal float BounceVelocity;
        internal float NormalImpulse;
        internal float TangentImpulse;
        internal float NormalImpulseBias;

        public Contact(Vector2 position, Geometry geometry, int feature)
        {
            Position = position;
            ID = new ContactID(geometry, feature);
            MassNormal = 0;
            MassTangent = 0;
            NormalVelocityBias = 0;
            BounceVelocity = 0;
            NormalImpulse = 0;
            TangentImpulse = 0;
            NormalImpulseBias = 0;
        }

        public override bool Equals(object obj)
        {
            if (obj is Contact)
                return Equals((Contact)obj);
            else
                return base.Equals(obj);
        }

        public bool Equals(Contact c)
        {
            return ID.Equals(c.ID);
        }

        public override int GetHashCode()
        {
            return ID.GetHashCode();
        }
    }
}
