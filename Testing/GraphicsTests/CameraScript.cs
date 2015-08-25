using System.Numerics;
using Myre.Graphics;
using System;

using Curve = Microsoft.Xna.Framework.Curve;
using CurveLoopType = Microsoft.Xna.Framework.CurveLoopType;
using CurveKey = Microsoft.Xna.Framework.CurveKey;

namespace GraphicsTests
{
    class Curve3D
    {
        public readonly Curve CurveX = new Curve();
        public readonly Curve CurveY = new Curve();
        public readonly Curve CurveZ = new Curve();

        public Curve3D()
        {
            CurveX.PostLoop = CurveLoopType.Oscillate;
            CurveY.PostLoop = CurveLoopType.Oscillate;
            CurveZ.PostLoop = CurveLoopType.Oscillate;

            CurveX.PreLoop = CurveLoopType.Oscillate;
            CurveY.PreLoop = CurveLoopType.Oscillate;
            CurveZ.PreLoop = CurveLoopType.Oscillate;
        }

        public void SetTangents()
        {
            for (int i = 0; i < CurveX.Keys.Count; i++)
            {
                int prevIndex = i - 1;
                if (prevIndex < 0) prevIndex = i;

                int nextIndex = i + 1;
                if (nextIndex == CurveX.Keys.Count) nextIndex = i;

                CurveKey prev = CurveX.Keys[prevIndex];
                CurveKey next = CurveX.Keys[nextIndex];
                CurveKey current = CurveX.Keys[i];
                SetCurveKeyTangent(ref prev, ref current, ref next);
                CurveX.Keys[i] = current;

                prev = CurveY.Keys[prevIndex];
                next = CurveY.Keys[nextIndex];
                current = CurveY.Keys[i];
                SetCurveKeyTangent(ref prev, ref current, ref next);
                CurveY.Keys[i] = current;

                prev = CurveZ.Keys[prevIndex];
                next = CurveZ.Keys[nextIndex];
                current = CurveZ.Keys[i];
                SetCurveKeyTangent(ref prev, ref current, ref next);
                CurveZ.Keys[i] = current;
            }
        }

        static void SetCurveKeyTangent(ref CurveKey prev, ref CurveKey cur, ref CurveKey next)
        {
            float dt = next.Position - prev.Position;
            float dv = next.Value - prev.Value;
            if (Math.Abs(dv) < float.Epsilon)
            {
                cur.TangentIn = 0;
                cur.TangentOut = 0;
            }
            else
            {
                // The in and out tangents should be equal to the slope between the adjacent keys.
                cur.TangentIn = dv * (cur.Position - prev.Position) / dt;
                cur.TangentOut = dv * (next.Position - cur.Position) / dt;
            }
        }

        public void AddPoint(Vector3 point, float time)
        {
            CurveX.Keys.Add(new CurveKey(time, point.X));
            CurveY.Keys.Add(new CurveKey(time, point.Y));
            CurveZ.Keys.Add(new CurveKey(time, point.Z));
        }

        public Vector3 GetPointOnCurve(float time)
        {
            Vector3 point = new Vector3 {
                X = CurveX.Evaluate(time),
                Y = CurveY.Evaluate(time),
                Z = CurveZ.Evaluate(time)
            };
            return point;
        }
    }

    class CameraScript
    {
        private readonly Camera _camera;
        private readonly Curve3D _positionCurve;
        private readonly Curve3D _lookatCurve;
        private float _time;

        public Vector3 Position
        {
            get;
            private set;
        }

        public Vector3 LookAt
        {
            get;
            private set;
        }

        public CameraScript(Camera camera)
        {
            _camera = camera;
            _positionCurve = new Curve3D();
            _lookatCurve = new Curve3D();
        }

        public void AddWaypoint(float time, Vector3 position, Vector3 lookat)
        {
            _positionCurve.AddPoint(position, time);
            _lookatCurve.AddPoint(lookat, time);
        }

        public void Initialise()
        {
            _positionCurve.SetTangents();
            _lookatCurve.SetTangents();
        }

        public void Update(float dt)
        {
            _time += dt;

            Position = _positionCurve.GetPointOnCurve(_time);
            LookAt = _lookatCurve.GetPointOnCurve(_time);

            _camera.View = Matrix4x4.CreateLookAt(Position, LookAt, Vector3.UnitY);
        }
    }
}
