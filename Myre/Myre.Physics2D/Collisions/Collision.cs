using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Myre.Extensions;

namespace Myre.Physics2D.Collisions
{
    public class Collision
    {
        private readonly static Stack<Collision> Pool = new Stack<Collision>();

        private Geometry a;
        private Geometry b;
        private Vector2 normal;
        private float penetrationDepth;
        private List<Contact> contacts;
        private List<Contact> newContacts;
        private bool initialised;

        private float frictionCoefficient;
        private float restitutionCoefficient;

        public Geometry A { get { return a; } }
        public Geometry B { get { return b; } }
        public List<Contact> Contacts { get { return contacts; } }
        public Vector2 Normal { get { return normal; } }
        public float PenetrationDepth { get { return penetrationDepth; } }

        public bool IsDead
        {
            get { return contacts.Count == 0; }
        }

        public bool IsActive
        {
            get { return !(a.Body.Sleeping && b.Body.Sleeping); }
        }

        #region temp variables
        Vector2 r1, r2;
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
            this.contacts = new List<Contact>();
            this.newContacts = new List<Contact>();
            this.normal = Vector2.Zero;
            this.penetrationDepth = 0;
            this.initialised = false;
        }

        public static Collision Create(Geometry a, Geometry b)
        {
            var collision = Pool.Count > 0 ? Pool.Pop() : new Collision();
            collision.a = a;
            collision.b = b;
            return collision;
        }

        public void FindContacts(SatTester tester, int maxContacts = 8)
        {
            if (!IsActive)
                return;

            // perform SAT collision detection
            SatResult? r = tester.FindIntersection(a, b);

            if (r == null)
            {
                contacts.Clear();
                return;
            }

            SatResult result = r.Value;
            normal = result.NormalAxis;
            penetrationDepth = result.Penetration;

            // find all vertices in a which are inside b, add them as contacts
            var aVertices = a.GetVertices(result.NormalAxis);
            for (int i = 0; i < aVertices.Length; i++)
            {
                if (newContacts.Count == maxContacts)
                    break;

                if (b.Contains(aVertices[i]))
                    newContacts.Add(new Contact(aVertices[i], a, i));
            }

            // find all vertices in b which are inside a, add them as contacts
            var bVertices = b.GetVertices(-result.NormalAxis);
            for (int i = 0; i < bVertices.Length; i++)
            {
                if (contacts.Count == maxContacts)
                    break;

                if (a.Contains(bVertices[i]))
                    newContacts.Add(new Contact(bVertices[i], b, i));
            }

            // add the deepest point if we found no others
            if (newContacts.Count == 0)
                newContacts.Add(new Contact(result.DeepestPoint, a, 0));

            // merge new contacts with the old ones
            MergeContacts();

            if (!initialised)
            {
                A.collidingWith.Add(B);
                B.collidingWith.Add(A);
                initialised = true;
            }
        }

        private void MergeContacts()
        {
            for (int i = 0; i < newContacts.Count; i++)
            {
                var contact = newContacts[i];

                var index = contacts.IndexOf(contact);
                if (index == -1)
                    continue;

                var previous = contacts[index];
                contact.normalImpulse = previous.normalImpulse;
                contact.tangentImpulse = previous.tangentImpulse;

                newContacts[i] = contact;
            }

            contacts.Clear();
            var tmp = contacts;
            contacts = newContacts;
            newContacts = tmp;
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

        public void Prepare(float allowedPenetration, float biasFactor, float inverseDT)
        {
            if (!IsActive)
                return;

            frictionCoefficient = (a.FrictionCoefficient + b.FrictionCoefficient) * 0.5f;
            restitutionCoefficient = (a.Restitution + b.Restitution) * 0.5f;

            for (int i = 0; i < contacts.Count; i++)
            {
                var contact = contacts[i];

                // copy + paste from farseer.. which is based on Box2D, and I cba to write yet another variation

                //calculate contact offset from body position
                var aPos = a.Body.Position;
                var bPos = b.Body.Position;
                Vector2.Subtract(ref contact.Position, ref aPos, out r1);
                Vector2.Subtract(ref contact.Position, ref bPos, out r2);

                //project normal onto offset vectors
                Vector2.Dot(ref r1, ref normal, out rn1);
                Vector2.Dot(ref r2, ref normal, out rn2);

                //calculate mass normal
                float invMassSum = (1f / a.Body.Mass) + (1f / b.Body.Mass);
                Vector2.Dot(ref r1, ref r1, out float1);
                Vector2.Dot(ref r2, ref r2, out float2);
                kNormal = invMassSum
                    + (float1 - rn1 * rn1) / a.Body.InertiaTensor
                    + (float2 - rn2 * rn2) / b.Body.InertiaTensor;
                contact.massNormal = 1f / kNormal;

                //float rnA = r1.X * normal.Y - r1.Y * normal.X;
                //float rnB = r2.X * normal.Y - r2.Y * normal.X;
                //rnA *= rnA;
                //rnB *= rnB;

                //float kNormal = invMassSum + rnA / A.Body.InertiaTensor + rnB / B.Body.InertiaTensor;
                //contact.massNormal = 1f / kNormal;

                //calculate mass tangent
                tangent = normal.Perpendicular();
                Vector2.Dot(ref r1, ref tangent, out rt1);
                Vector2.Dot(ref r2, ref tangent, out rt2);

                Vector2.Dot(ref r1, ref r1, out float1);
                Vector2.Dot(ref r2, ref r2, out float2);
                kTangent = invMassSum
                    + (float1 - rt1 * rt1) / a.Body.InertiaTensor
                    + (float2 - rt2 * rt2) / b.Body.InertiaTensor;
                contact.massTangent = 1f / kTangent;

                //float rtA = r1.X * tangent.Y - r1.Y * tangent.X;
                //float rtB = r2.X * tangent.Y - r2.Y * tangent.X;
                //rtA *= rtA;
                //rtB *= rtB;

                //float kTangent = invMassSum + rnA / A.Body.InertiaTensor + rnB / B.Body.InertiaTensor;
                //contact.massTangent = 1f / kTangent;

                //calc velocity bias
                max = Math.Max(0, penetrationDepth - allowedPenetration);
                contact.normalVelocityBias = biasFactor * inverseDT * max;

                //calc bounce velocity
                vec1 = a.Body.GetVelocityAtOffset(r1);
                vec2 = b.Body.GetVelocityAtOffset(r2);
                Vector2.Subtract(ref vec2, ref vec1, out dv);

                //calc velocity difference along contact normal
                Vector2.Dot(ref dv, ref normal, out vn);
                contact.bounceVelocity = vn * restitutionCoefficient;

                //apply accumulated impulse
                Vector2.Multiply(ref normal, contact.normalImpulse, out vec1);
                Vector2.Multiply(ref tangent, contact.tangentImpulse, out vec2);
                Vector2.Add(ref vec1, ref vec2, out impulse);

                b.Body.CollisionImpulse(b, a, ref impulse, ref r2);

                Vector2.Multiply(ref impulse, -1, out impulse);
                a.Body.CollisionImpulse(b, a, ref impulse, ref r2);

                contact.normalImpulseBias = 0;

                contacts[i] = contact;
            }
        }

        public void Iterate()
        {
            if (!IsActive)
                return;

            var aPos = a.Body.Position;
            var bPos = b.Body.Position;

            for (int i = 0; i < contacts.Count; i++)
            {
                var contact = contacts[i];

                // copy + paste from farseer.. which is based on Box2D, and I cba to write yet another variation

                #region INLINE: Vector2.Subtract(ref contact.Position, ref geometryA.body.position, out r1);

                r1.X = contact.Position.X - aPos.X;
                r1.Y = contact.Position.Y - aPos.Y;

                #endregion

                #region INLINE: Vector2.Subtract(ref contact.Position, ref geometryB.body.position, out r2);

                r2.X = contact.Position.X - bPos.X;
                r2.Y = contact.Position.Y - bPos.Y;

                #endregion

                //calc velocity difference
                vec1 = a.Body.GetVelocityAtOffset(r1);
                vec2 = b.Body.GetVelocityAtOffset(r2);

                #region INLINE: Vector2.Subtract(ref vec2, ref vec1, out dv);

                dv.X = vec2.X - vec1.X;
                dv.Y = vec2.Y - vec1.Y;

                #endregion

                //calc velocity difference along contact normal
                #region INLINE: Vector2.Dot(ref dv, ref normal, out vn);

                vn = (dv.X * normal.X) + (dv.Y * normal.Y);

                #endregion

                normalImpulse = contact.massNormal * -(vn + contact.bounceVelocity); //uncomment for preserve momentum

                //clamp accumulated impulse
                oldNormalImpulse = contact.normalImpulse;
                contact.normalImpulse = Math.Max(oldNormalImpulse + normalImpulse, 0);
                normalImpulse = contact.normalImpulse - oldNormalImpulse;

                //apply contact impulse
                #region INLINE: Vector2.Multiply(ref normal, normalImpulse, out impulse);

                impulse.X = normal.X * normalImpulse;
                impulse.Y = normal.Y * normalImpulse;

                #endregion

                b.Body.ApplyImpulseAtOffset(ref impulse, ref r2);

                #region INLINE: Vector2.Multiply(ref impulse, -1, out impulse);

                impulse.X = impulse.X * -1;
                impulse.Y = impulse.Y * -1;

                #endregion

                a.Body.ApplyImpulseAtOffset(ref impulse, ref r1);

                //calc velocity bias difference (bias preserves momentum)
                vec1 = a.Body.GetVelocityBiasAtOffset(r1);
                vec2 = b.Body.GetVelocityBiasAtOffset(r2);

                #region INLINE: Vector2.Subtract(ref vec2, ref vec1, out dv);

                dv.X = vec2.X - vec1.X;
                dv.Y = vec2.Y - vec1.Y;

                #endregion

                //calc velocity bias along contact normal
                #region INLINE: Vector2.Dot(ref dv, ref normal, out normalVelocityBias);

                normalVelocityBias = (dv.X * normal.X) + (dv.Y * normal.Y);

                #endregion

                normalImpulseBias = contact.massNormal * (-normalVelocityBias + contact.normalVelocityBias);
                normalImpulseBiasOriginal = contact.normalImpulseBias;
                contact.normalImpulseBias = Math.Max(normalImpulseBiasOriginal + normalImpulseBias, 0);
                normalImpulseBias = contact.normalImpulseBias - normalImpulseBiasOriginal;

                #region INLINE: Vector2.Multiply(ref normal, normalImpulseBias, out impulseBias);

                impulseBias.X = normal.X * normalImpulseBias;
                impulseBias.Y = normal.Y * normalImpulseBias;

                #endregion

                //apply bias impulse
                b.Body.ApplyBiasImpulseAtOffset(ref impulseBias, ref r2);

                #region INLINE: Vector2.Multiply(ref impulseBias, -1, out impulseBias);

                impulseBias.X = impulseBias.X * -1;
                impulseBias.Y = impulseBias.Y * -1;

                #endregion

                a.Body.ApplyBiasImpulseAtOffset(ref impulseBias, ref r1);

                //calc relative velocity at contact.
                vec1 = a.Body.GetVelocityAtOffset(r1);
                vec2 = b.Body.GetVelocityAtOffset(r2);

                #region INLINE: Vector2.Subtract(ref _vec2, ref _vec1, out _dv);

                dv.X = vec2.X - vec1.X;
                dv.Y = vec2.Y - vec1.Y;

                #endregion

                //compute friction impulse
                maxTangentImpulse = frictionCoefficient * contact.normalImpulse;
                float1 = 1;

                #region INLINE: Calculator.Cross(ref normal, ref float1, out tangent);

                tangent.X = float1 * normal.Y;
                tangent.Y = -float1 * normal.X;

                #endregion

                #region INLINE: Vector2.Dot(ref dv, ref tangent, out vt);

                vt = (dv.X * tangent.X) + (dv.Y * tangent.Y);

                #endregion

                tangentImpulse = contact.massTangent * (-vt);

                //clamp friction
                oldTangentImpulse = contact.tangentImpulse;
                contact.tangentImpulse = MathHelper.Clamp(oldTangentImpulse + tangentImpulse, -maxTangentImpulse,
                                                           maxTangentImpulse);
                tangentImpulse = contact.tangentImpulse - oldTangentImpulse;

                //apply friction impulse
                #region INLINE:Vector2.Multiply(ref tangent, tangentImpulse, out impulse);

                impulse.X = tangent.X * tangentImpulse;
                impulse.Y = tangent.Y * tangentImpulse;

                #endregion

                //apply impulse
                b.Body.ApplyImpulseAtOffset(ref impulse, ref r2);

                #region INLINE: Vector2.Multiply(ref impulse, -1, out impulse);

                impulse.X = impulse.X * -1;
                impulse.Y = impulse.Y * -1;

                #endregion

                a.Body.ApplyImpulseAtOffset(ref impulse, ref r1);

                contacts[i] = contact;

                //System.Diagnostics.Debug.WriteLine(string.Format("Contact {0}: point:{4}, normal:{1}, penetration:{2}, impulse:{3}", i, normal, penetrationDepth, contact.normalImpulse, contact.Position));
            }
            //System.Diagnostics.Debug.WriteLine("");
        }

        public void Dispose()
        {
            contacts.Clear();
            newContacts.Clear();

            A.collidingWith.Remove(B);
            B.collidingWith.Remove(A);

            initialised = false;

            Pool.Push(this);
        }

        public override int GetHashCode()
        {
            return a.GetHashCode() + b.GetHashCode();
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
            return this.a == obj.a
                && this.b == obj.b;
        }
    }
}
