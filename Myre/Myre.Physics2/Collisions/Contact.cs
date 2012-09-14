using Microsoft.Xna.Framework;

namespace Myre.Physics2.Collisions
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
        internal float massNormal;
        internal float massTangent;
        internal float normalVelocityBias;
        internal float bounceVelocity;
        internal float normalImpulse;
        internal float tangentImpulse;
        internal float normalImpulseBias;

        public Contact(Vector2 position, Geometry geometry, int feature)
        {
            Position = position;
            ID = new ContactID(geometry, feature);
            massNormal = 0;
            massTangent = 0;
            normalVelocityBias = 0;
            bounceVelocity = 0;
            normalImpulse = 0;
            tangentImpulse = 0;
            normalImpulseBias = 0;
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
