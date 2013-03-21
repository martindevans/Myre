using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Myre.Extensions;

namespace Myre.Physics2D.Collisions
{
    public class Collision
    {
        private readonly static Stack<Collision> _pool = new Stack<Collision>();

        private Vector2 _normal;
        private List<Contact> _newContacts;
        private bool _initialised;

        private float _frictionCoefficient;
        private float _restitutionCoefficient;

        public Geometry A { get; private set; }
        public Geometry B { get; private set; }
        public List<Contact> Contacts { get; private set; }
        public Vector2 Normal { get { return _normal; } }
        public float PenetrationDepth { get; private set; }

        public bool IsDead
        {
            get { return Contacts.Count == 0; }
        }

        public bool IsActive
        {
            get { return !(A.Body.Sleeping && B.Body.Sleeping); }
        }

        #region temp variables
        Vector2 _r1, _r2;
        float _rn1, _rn2;
        float _float1, _float2;
        float _kNormal;
        float _rt1, _rt2;
        Vector2 _tangent;
        float _kTangent;
        Vector2 _vec1, _vec2;
        Vector2 _dv;
        float _vn;
        Vector2 _impulse;
        float _max;
        float _normalImpulse;
        float _oldNormalImpulse;
        float _normalVelocityBias;
        float _normalImpulseBias;
        float _normalImpulseBiasOriginal;
        Vector2 _impulseBias;
        float _maxTangentImpulse;
        float _vt;
        float _tangentImpulse;
        float _oldTangentImpulse;
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
            collision.A = a;
            collision.B = b;
            return collision;
        }

        public void FindContacts(SatTester tester, int maxContacts)
        {
            if (!IsActive)
                return;

            // perform SAT collision detection
            SatResult? r = tester.FindIntersection(A, B);

            if (r == null)
            {
                Contacts.Clear();
                return;
            }

            SatResult result = r.Value;
            _normal = result.NormalAxis;
            PenetrationDepth = result.Penetration;

            // find all vertices in a which are inside b, add them as contacts
            var aVertices = A.GetVertices(result.NormalAxis);
            for (int i = 0; i < aVertices.Length; i++)
            {
                if (_newContacts.Count == maxContacts)
                    break;

                if (B.Contains(aVertices[i]))
                    _newContacts.Add(new Contact(aVertices[i], A, i));
            }

            // find all vertices in b which are inside a, add them as contacts
            var bVertices = B.GetVertices(-result.NormalAxis);
            for (int i = 0; i < bVertices.Length; i++)
            {
                if (Contacts.Count == maxContacts)
                    break;

                if (A.Contains(bVertices[i]))
                    _newContacts.Add(new Contact(bVertices[i], B, i));
            }

            // add the deepest point if we found no others
            if (_newContacts.Count == 0)
                _newContacts.Add(new Contact(result.DeepestPoint, A, 0));

            // merge new contacts with the old ones
            MergeContacts();

            if (!_initialised)
            {
                A.collidingWith.Add(B);
                B.collidingWith.Add(A);
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

            _frictionCoefficient = (A.FrictionCoefficient + B.FrictionCoefficient) * 0.5f;
            _restitutionCoefficient = (A.Restitution + B.Restitution) * 0.5f;

            for (int i = 0; i < Contacts.Count; i++)
            {
                var contact = Contacts[i];

                // copy + paste from farseer.. which is based on Box2D, and I cba to write yet another variation

                //calculate contact offset from body position
                var aPos = A.Body.Position;
                var bPos = B.Body.Position;
                Vector2.Subtract(ref contact.Position, ref aPos, out _r1);
                Vector2.Subtract(ref contact.Position, ref bPos, out _r2);

                //project normal onto offset vectors
                Vector2.Dot(ref _r1, ref _normal, out _rn1);
                Vector2.Dot(ref _r2, ref _normal, out _rn2);

                //calculate mass normal
                float invMassSum = (1f / A.Body.Mass) + (1f / B.Body.Mass);
                Vector2.Dot(ref _r1, ref _r1, out _float1);
                Vector2.Dot(ref _r2, ref _r2, out _float2);
                _kNormal = invMassSum
                    + (_float1 - _rn1 * _rn1) / A.Body.InertiaTensor
                    + (_float2 - _rn2 * _rn2) / B.Body.InertiaTensor;
                contact.MassNormal = 1f / _kNormal;

                //float rnA = r1.X * normal.Y - r1.Y * normal.X;
                //float rnB = r2.X * normal.Y - r2.Y * normal.X;
                //rnA *= rnA;
                //rnB *= rnB;

                //float kNormal = invMassSum + rnA / A.Body.InertiaTensor + rnB / B.Body.InertiaTensor;
                //contact.massNormal = 1f / kNormal;

                //calculate mass tangent
                _tangent = _normal.Perpendicular();
                Vector2.Dot(ref _r1, ref _tangent, out _rt1);
                Vector2.Dot(ref _r2, ref _tangent, out _rt2);

                Vector2.Dot(ref _r1, ref _r1, out _float1);
                Vector2.Dot(ref _r2, ref _r2, out _float2);
                _kTangent = invMassSum
                    + (_float1 - _rt1 * _rt1) / A.Body.InertiaTensor
                    + (_float2 - _rt2 * _rt2) / B.Body.InertiaTensor;
                contact.MassTangent = 1f / _kTangent;

                //float rtA = r1.X * tangent.Y - r1.Y * tangent.X;
                //float rtB = r2.X * tangent.Y - r2.Y * tangent.X;
                //rtA *= rtA;
                //rtB *= rtB;

                //float kTangent = invMassSum + rnA / A.Body.InertiaTensor + rnB / B.Body.InertiaTensor;
                //contact.massTangent = 1f / kTangent;

                //calc velocity bias
                _max = Math.Max(0, PenetrationDepth - allowedPenetration);
                contact.NormalVelocityBias = biasFactor * inverseDt * _max;

                //calc bounce velocity
                _vec1 = A.Body.GetVelocityAtOffset(_r1);
                _vec2 = B.Body.GetVelocityAtOffset(_r2);
                Vector2.Subtract(ref _vec2, ref _vec1, out _dv);

                //calc velocity difference along contact normal
                Vector2.Dot(ref _dv, ref _normal, out _vn);
                contact.BounceVelocity = _vn * _restitutionCoefficient;

                //apply accumulated impulse
                Vector2.Multiply(ref _normal, contact.NormalImpulse, out _vec1);
                Vector2.Multiply(ref _tangent, contact.TangentImpulse, out _vec2);
                Vector2.Add(ref _vec1, ref _vec2, out _impulse);

                B.Body.CollisionImpulse(B, A, ref _impulse, ref _r2);

                Vector2.Multiply(ref _impulse, -1, out _impulse);
                A.Body.CollisionImpulse(B, A, ref _impulse, ref _r2);

                contact.NormalImpulseBias = 0;

                Contacts[i] = contact;
            }
        }

        public void Iterate()
        {
            if (!IsActive)
                return;

            var aPos = A.Body.Position;
            var bPos = B.Body.Position;

            for (int i = 0; i < Contacts.Count; i++)
            {
                var contact = Contacts[i];

                // copy + paste from farseer.. which is based on Box2D, and I cba to write yet another variation

                #region INLINE: Vector2.Subtract(ref contact.Position, ref geometryA.body.position, out r1);

                _r1.X = contact.Position.X - aPos.X;
                _r1.Y = contact.Position.Y - aPos.Y;

                #endregion

                #region INLINE: Vector2.Subtract(ref contact.Position, ref geometryB.body.position, out r2);

                _r2.X = contact.Position.X - bPos.X;
                _r2.Y = contact.Position.Y - bPos.Y;

                #endregion

                //calc velocity difference
                _vec1 = A.Body.GetVelocityAtOffset(_r1);
                _vec2 = B.Body.GetVelocityAtOffset(_r2);

                #region INLINE: Vector2.Subtract(ref vec2, ref vec1, out dv);

                _dv.X = _vec2.X - _vec1.X;
                _dv.Y = _vec2.Y - _vec1.Y;

                #endregion

                //calc velocity difference along contact normal
                #region INLINE: Vector2.Dot(ref dv, ref normal, out vn);

                _vn = (_dv.X * _normal.X) + (_dv.Y * _normal.Y);

                #endregion

                _normalImpulse = contact.MassNormal * -(_vn + contact.BounceVelocity); //uncomment for preserve momentum

                //clamp accumulated impulse
                _oldNormalImpulse = contact.NormalImpulse;
                contact.NormalImpulse = Math.Max(_oldNormalImpulse + _normalImpulse, 0);
                _normalImpulse = contact.NormalImpulse - _oldNormalImpulse;

                //apply contact impulse
                #region INLINE: Vector2.Multiply(ref normal, normalImpulse, out impulse);

                _impulse.X = _normal.X * _normalImpulse;
                _impulse.Y = _normal.Y * _normalImpulse;

                #endregion

                B.Body.ApplyImpulseAtOffset(ref _impulse, ref _r2);

                #region INLINE: Vector2.Multiply(ref impulse, -1, out impulse);

                _impulse.X = _impulse.X * -1;
                _impulse.Y = _impulse.Y * -1;

                #endregion

                A.Body.ApplyImpulseAtOffset(ref _impulse, ref _r1);

                //calc velocity bias difference (bias preserves momentum)
                _vec1 = A.Body.GetVelocityBiasAtOffset(_r1);
                _vec2 = B.Body.GetVelocityBiasAtOffset(_r2);

                #region INLINE: Vector2.Subtract(ref vec2, ref vec1, out dv);

                _dv.X = _vec2.X - _vec1.X;
                _dv.Y = _vec2.Y - _vec1.Y;

                #endregion

                //calc velocity bias along contact normal
                #region INLINE: Vector2.Dot(ref dv, ref normal, out normalVelocityBias);

                _normalVelocityBias = (_dv.X * _normal.X) + (_dv.Y * _normal.Y);

                #endregion

                _normalImpulseBias = contact.MassNormal * (-_normalVelocityBias + contact.NormalVelocityBias);
                _normalImpulseBiasOriginal = contact.NormalImpulseBias;
                contact.NormalImpulseBias = Math.Max(_normalImpulseBiasOriginal + _normalImpulseBias, 0);
                _normalImpulseBias = contact.NormalImpulseBias - _normalImpulseBiasOriginal;

                #region INLINE: Vector2.Multiply(ref normal, normalImpulseBias, out impulseBias);

                _impulseBias.X = _normal.X * _normalImpulseBias;
                _impulseBias.Y = _normal.Y * _normalImpulseBias;

                #endregion

                //apply bias impulse
                B.Body.ApplyBiasImpulseAtOffset(ref _impulseBias, ref _r2);

                #region INLINE: Vector2.Multiply(ref impulseBias, -1, out impulseBias);

                _impulseBias.X = _impulseBias.X * -1;
                _impulseBias.Y = _impulseBias.Y * -1;

                #endregion

                A.Body.ApplyBiasImpulseAtOffset(ref _impulseBias, ref _r1);

                //calc relative velocity at contact.
                _vec1 = A.Body.GetVelocityAtOffset(_r1);
                _vec2 = B.Body.GetVelocityAtOffset(_r2);

                #region INLINE: Vector2.Subtract(ref _vec2, ref _vec1, out _dv);

                _dv.X = _vec2.X - _vec1.X;
                _dv.Y = _vec2.Y - _vec1.Y;

                #endregion

                //compute friction impulse
                _maxTangentImpulse = _frictionCoefficient * contact.NormalImpulse;
                _float1 = 1;

                #region INLINE: Calculator.Cross(ref normal, ref float1, out tangent);

                _tangent.X = _float1 * _normal.Y;
                _tangent.Y = -_float1 * _normal.X;

                #endregion

                #region INLINE: Vector2.Dot(ref dv, ref tangent, out vt);

                _vt = (_dv.X * _tangent.X) + (_dv.Y * _tangent.Y);

                #endregion

                _tangentImpulse = contact.MassTangent * (-_vt);

                //clamp friction
                _oldTangentImpulse = contact.TangentImpulse;
                contact.TangentImpulse = MathHelper.Clamp(_oldTangentImpulse + _tangentImpulse, -_maxTangentImpulse,
                                                           _maxTangentImpulse);
                _tangentImpulse = contact.TangentImpulse - _oldTangentImpulse;

                //apply friction impulse
                #region INLINE:Vector2.Multiply(ref tangent, tangentImpulse, out impulse);

                _impulse.X = _tangent.X * _tangentImpulse;
                _impulse.Y = _tangent.Y * _tangentImpulse;

                #endregion

                //apply impulse
                B.Body.ApplyImpulseAtOffset(ref _impulse, ref _r2);

                #region INLINE: Vector2.Multiply(ref impulse, -1, out impulse);

                _impulse.X = _impulse.X * -1;
                _impulse.Y = _impulse.Y * -1;

                #endregion

                A.Body.ApplyImpulseAtOffset(ref _impulse, ref _r1);

                Contacts[i] = contact;

                //System.Diagnostics.Debug.WriteLine(string.Format("Contact {0}: point:{4}, normal:{1}, penetration:{2}, impulse:{3}", i, normal, penetrationDepth, contact.normalImpulse, contact.Position));
            }
            //System.Diagnostics.Debug.WriteLine("");
        }

        public void Dispose()
        {
            Contacts.Clear();
            _newContacts.Clear();

            A.collidingWith.Remove(B);
            B.collidingWith.Remove(A);

            _initialised = false;

            _pool.Push(this);
        }

        public override int GetHashCode()
        {
            return A.GetHashCode() + B.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            var a = obj as Collision;
            if (a != null)
                return Equals(a);
            else
                return ReferenceEquals(this, obj);
        }

        public bool Equals(Collision obj)
        {
            return A == obj.A
                && B == obj.B;
        }
    }
}
