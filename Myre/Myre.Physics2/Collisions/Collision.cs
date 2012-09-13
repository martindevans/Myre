using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Myre.Extensions;

namespace Myre.Physics2.Collisions
{
    public class Collision
    {
        private readonly static Stack<Collision> _pool = new Stack<Collision>();

        private Vector2 _normal;
        private List<Contact> _newContacts;
        private bool _initialised;

        private float _frictionCoefficient;
        private float _restitutionCoefficient;

        private Geometry _geometryA;
        public Geometry GeometryA { get { return _geometryA; } }

        private Geometry _geometryB;
        public Geometry GeometryB { get { return _geometryB; } }

        public List<Contact> Contacts { get; private set; }
        public Vector2 Normal { get { return _normal; } }
        public float PenetrationDepth { get; private set; }

        public bool IsDead
        {
            get { return Contacts.Count == 0; }
        }

        public bool IsActive
        {
            get { return !(GeometryA.Body.Sleeping && GeometryB.Body.Sleeping); }
        }

        #region temp variables
        float rn1, rn2;
        float float1, float2;
        float kNormal;
        float rt1, rt2;
        Vector2 tangent;
        float kTangent;
        Vector2 vec1, vec2;
        Vector2 dv;
        float vn;
        Vector2 impulse;
        float max;
        float normalImpulse;
        float oldNormalImpulse;
        float normalVelocityBias;
        float normalImpulseBias;
        float normalImpulseBiasOriginal;
        Vector2 impulseBias;
        float maxTangentImpulse;
        float vt;
        float tangentImpulse;
        float oldTangentImpulse;
        #endregion

        private Collision()
        {
            Contacts = new List<Contact>();
            _newContacts = new List<Contact>();
            _normal = Vector2.Zero;
            PenetrationDepth = 0;
            _initialised = false;
        }

        public static Collision Create(Geometry a, Geometry b)
        {
            var collision = _pool.Count > 0 ? _pool.Pop() : new Collision();
            collision._geometryA = a;
            collision._geometryB = b;
            return collision;
        }

        public void FindContacts(SatTester tester, int maxContacts = 8)
        {
            if (!IsActive)
                return;

            // perform SAT collision detection
            SatResult? r = tester.FindIntersection(GeometryA, GeometryB);

            if (r == null)
            {
                Contacts.Clear();
                return;
            }
            
            SatResult result = r.Value;
            _normal = result.NormalAxis;
            PenetrationDepth = result.Penetration;

            // find all vertices in a which are inside b, add them as contacts
            var aVertices = GeometryA.GetVertices(result.NormalAxis);
            for (int i = 0; i < aVertices.Length; i++)
            {
                if (_newContacts.Count == maxContacts)
                    break;

                if (GeometryB.Contains(aVertices[i]))
                    _newContacts.Add(new Contact(aVertices[i], GeometryA, i));
            }

            // find all vertices in b which are inside a, add them as contacts
            var bVertices = GeometryB.GetVertices(-result.NormalAxis);
            for (int i = 0; i < bVertices.Length; i++)
            {
                if (Contacts.Count == maxContacts)
                    break;

                if (GeometryA.Contains(bVertices[i]))
                    _newContacts.Add(new Contact(bVertices[i], GeometryB, i));
            }

            // add the deepest point if we found no others
            if (_newContacts.Count == 0)
                _newContacts.Add(new Contact(result.DeepestPoint, GeometryA, 0));

            // merge new contacts with the old ones
            MergeContacts();

            if (!_initialised)
            {
                GeometryA.collidingWith.Add(GeometryB);
                GeometryB.collidingWith.Add(GeometryA);
                _initialised = true;
            }
        }

        private void MergeContacts()
        {
            for (int i = 0; i < _newContacts.Count; i++)
            {
                var contact = _newContacts[i];

                var index = Contacts.IndexOf(contact);
                if (index == -1)
                    continue;

                var previous = Contacts[index];
                contact.NormalImpulse = previous.NormalImpulse;
                contact.TangentImpulse = previous.TangentImpulse;

                _newContacts[i] = contact;
            }

            Contacts.Clear();
            var tmp = Contacts;
            Contacts = _newContacts;
            _newContacts = tmp;
        }

        //public bool ShouldActivateBody(out DynamicPhysics body)
        //{
        //    body = null;

        //    if (contacts.Count == 0)
        //        return false;

        //    if (a.Body.Sleeping && !b.Body.Sleeping)
        //    {
        //        body = b.Body;
        //        return true;
        //    }

        //    if (!a.Body.Sleeping && b.Body.Sleeping)
        //    {
        //        body = a.Body;
        //        return true;
        //    }

        //    return false;
        //}

        public void Prepare(float allowedPenetration, float biasFactor, float inverseDt)
        {
            if (!IsActive)
                return;
            
            _frictionCoefficient = (GeometryA.FrictionCoefficient + GeometryB.FrictionCoefficient) * 0.5f;
            _restitutionCoefficient = (GeometryA.Restitution + GeometryB.Restitution) * 0.5f;

            for (int i = 0; i < Contacts.Count; i++)
            {
                var contact = Contacts[i];

                // copy + paste from farseer.. which is based on Box2D, and I cba to write yet another variation

                //calculate contact offset from body position
                var aPos = GeometryA.Body.Position;
                var bPos = GeometryB.Body.Position;
                Vector2 r1;
                Vector2 r2;
                Vector2.Subtract(ref contact.Position, ref aPos, out r1);
                Vector2.Subtract(ref contact.Position, ref bPos, out r2);

                //project normal onto offset vectors
                Vector2.Dot(ref r1, ref _normal, out rn1);
                Vector2.Dot(ref r2, ref _normal, out rn2);

                //calculate mass normal
                float invMassSum = (1f / GeometryA.Body.Mass) + (1f / GeometryB.Body.Mass);
                Vector2.Dot(ref r1, ref r1, out float1);
                Vector2.Dot(ref r2, ref r2, out float2);
                kNormal = invMassSum
                    + (float1 - rn1 * rn1) / GeometryA.Body.InertiaTensor
                    + (float2 - rn2 * rn2) / GeometryB.Body.InertiaTensor;
                contact.MassNormal = 1f / kNormal;

                //float rnA = r1.X * normal.Y - r1.Y * normal.X;
                //float rnB = r2.X * normal.Y - r2.Y * normal.X;
                //rnA *= rnA;
                //rnB *= rnB;

                //float kNormal = invMassSum + rnA / A.Body.InertiaTensor + rnB / B.Body.InertiaTensor;
                //contact.massNormal = 1f / kNormal;

                //calculate mass tangent
                tangent = _normal.Perpendicular();
                Vector2.Dot(ref r1, ref tangent, out rt1);
                Vector2.Dot(ref r2, ref tangent, out rt2);

                Vector2.Dot(ref r1, ref r1, out float1);
                Vector2.Dot(ref r2, ref r2, out float2);
                kTangent = invMassSum
                    + (float1 - rt1 * rt1) / GeometryA.Body.InertiaTensor
                    + (float2 - rt2 * rt2) / GeometryB.Body.InertiaTensor;
                contact.MassTangent = 1f / kTangent;

                //float rtA = r1.X * tangent.Y - r1.Y * tangent.X;
                //float rtB = r2.X * tangent.Y - r2.Y * tangent.X;
                //rtA *= rtA;
                //rtB *= rtB;

                //float kTangent = invMassSum + rnA / A.Body.InertiaTensor + rnB / B.Body.InertiaTensor;
                //contact.massTangent = 1f / kTangent;

                //calc velocity bias
                max = Math.Max(0, PenetrationDepth - allowedPenetration);
                contact.NormalVelocityBias = biasFactor * inverseDt * max;

                //calc bounce velocity
                vec1 = GeometryA.Body.GetVelocityAtOffset(r1);
                vec2 = GeometryB.Body.GetVelocityAtOffset(r2);
                Vector2.Subtract(ref vec2, ref vec1, out dv);

                //calc velocity difference along contact normal
                Vector2.Dot(ref dv, ref _normal, out vn);
                contact.BounceVelocity = vn * _restitutionCoefficient;

                //apply accumulated impulse
                Vector2.Multiply(ref _normal, contact.NormalImpulse, out vec1);
                Vector2.Multiply(ref tangent, contact.TangentImpulse, out vec2);
                Vector2.Add(ref vec1, ref vec2, out impulse);

                GeometryB.Body.ApplyImpulseAtOffset(ref impulse, ref r2);

                Vector2.Multiply(ref impulse, -1, out impulse);
                GeometryA.Body.ApplyImpulseAtOffset(ref impulse, ref r1);

                contact.NormalImpulseBias = 0;

                Contacts[i] = contact;
            }
        }

        public void Iterate()
        {
            if (!IsActive)
                return;

            var aPos = GeometryA.Body.Position;
            var bPos = GeometryB.Body.Position;

            for (int i = 0; i < Contacts.Count; i++)
            {
                var contact = Contacts[i];

                // copy + paste from farseer.. which is based on Box2D, and I cba to write yet another variation

                Vector2 r1 = contact.Position - aPos;
                Vector2 r2 = contact.Position - bPos;

                //calc velocity difference
                vec1 = GeometryA.Body.GetVelocityAtOffset(r1);
                vec2 = GeometryB.Body.GetVelocityAtOffset(r2);

                Vector2.Subtract(ref vec2, ref vec1, out dv);

                //calc velocity difference along contact normal
                Vector2.Dot(ref dv, ref _normal, out vn);

                normalImpulse = contact.MassNormal * -(vn + contact.BounceVelocity); //uncomment for preserve momentum

                //clamp accumulated impulse
                oldNormalImpulse = contact.NormalImpulse;
                contact.NormalImpulse = Math.Max(oldNormalImpulse + normalImpulse, 0);
                normalImpulse = contact.NormalImpulse - oldNormalImpulse;

                //apply contact impulse
                Vector2.Multiply(ref _normal, normalImpulse, out impulse);

                GeometryB.Body.ApplyImpulseAtOffset(ref impulse, ref r2);

                Vector2.Multiply(ref impulse, -1, out impulse);

                GeometryA.Body.ApplyImpulseAtOffset(ref impulse, ref r1);

                //calc velocity bias difference (bias preserves momentum)
                vec1 = GeometryA.Body.GetVelocityBiasAtOffset(r1);
                vec2 = GeometryB.Body.GetVelocityBiasAtOffset(r2);

                Vector2.Subtract(ref vec2, ref vec1, out dv);

                //calc velocity bias along contact normal
                Vector2.Dot(ref dv, ref _normal, out normalVelocityBias);

                normalImpulseBias = contact.MassNormal * (-normalVelocityBias + contact.NormalVelocityBias);
                normalImpulseBiasOriginal = contact.NormalImpulseBias;
                contact.NormalImpulseBias = Math.Max(normalImpulseBiasOriginal + normalImpulseBias, 0);
                normalImpulseBias = contact.NormalImpulseBias - normalImpulseBiasOriginal;

                Vector2.Multiply(ref _normal, normalImpulseBias, out impulseBias);

                //apply bias impulse
                GeometryB.Body.ApplyBiasImpulseAtOffset(ref impulseBias, ref r2);

                Vector2.Multiply(ref impulseBias, -1, out impulseBias);

                GeometryA.Body.ApplyBiasImpulseAtOffset(ref impulseBias, ref r1);

                //calc relative velocity at contact.
                vec1 = GeometryA.Body.GetVelocityAtOffset(r1);
                vec2 = GeometryB.Body.GetVelocityAtOffset(r2);

                Vector2.Subtract(ref vec2, ref vec1, out dv);

                //compute friction impulse
                maxTangentImpulse = _frictionCoefficient * contact.NormalImpulse;
                float1 = 1;

                #region INLINE: Calculator.Cross(ref normal, ref float1, out tangent);
                tangent.X = float1 * _normal.Y;
                tangent.Y = -float1 * _normal.X;
                #endregion

                Vector2.Dot(ref dv, ref tangent, out vt);

                tangentImpulse = contact.MassTangent * (-vt);

                //clamp friction
                oldTangentImpulse = contact.TangentImpulse;
                contact.TangentImpulse = MathHelper.Clamp(oldTangentImpulse + tangentImpulse, -maxTangentImpulse,
                                                           maxTangentImpulse);
                tangentImpulse = contact.TangentImpulse - oldTangentImpulse;

                //apply friction impulse
                Vector2.Multiply(ref tangent, tangentImpulse, out impulse);

                //apply impulse
                GeometryB.Body.ApplyImpulseAtOffset(ref impulse, ref r2);

                Vector2.Multiply(ref impulse, -1, out impulse);

                GeometryA.Body.ApplyImpulseAtOffset(ref impulse, ref r1);

                Contacts[i] = contact;

                //System.Diagnostics.Debug.WriteLine(string.Format("Contact {0}: point:{4}, normal:{1}, penetration:{2}, impulse:{3}", i, normal, penetrationDepth, contact.normalImpulse, contact.Position));
            }
            //System.Diagnostics.Debug.WriteLine("");
        }

        public void Dispose()
        {
            Contacts.Clear();
            _newContacts.Clear();

            GeometryA.collidingWith.Remove(GeometryB);
            GeometryB.collidingWith.Remove(GeometryA);

            _initialised = false;

            _pool.Push(this);
        }

        public override int GetHashCode()
        {
            return GeometryA.GetHashCode() + GeometryB.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (obj is Collision)
                return Equals((Collision)obj);
            else
                return base.Equals(obj);
        }

        public bool Equals(Collision obj)
        {
            return this.GeometryA == obj.GeometryA
                && this.GeometryB == obj.GeometryB;
        }
    }
}
