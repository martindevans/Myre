using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Myre.Entities;
using Myre.Entities.Behaviours;
using Myre.Entities.Extensions;

namespace Myre.Graphics.Lighting
{
    /// <summary>
    /// A light which is a panel of light (textured)
    /// </summary>
    public class PlaneLight
        :Behaviour
    {
        public const string TEXTURE_NAME = "texture";
        public const string POSITION_NAME = "position";
        public const string RANGE_NAME = "range";
        public const string EXTENTS_NAME = "extents";
        public const string NORMAL_NAME = "normal";
        public const string BINORMAL_NAME = "binormal";

        private Property<Texture2D> _texture;
        private Property<Vector3> _position;
        private Property<float> _range;
        private Property<Vector2> _extents;
        private Property<Vector3> _normal;
        private Property<Vector3> _binormal;

        public Texture2D Texture
        {
            get { return _texture.Value; }
            set { _texture.Value = value; }
        }

        public Vector3 Position
        {
            get { return _position.Value; }
            set { _position.Value = value; }
        }

        public float Range
        {
            get { return _range.Value; }
            set { _range.Value = value; }
        }

        public Vector2 Extents
        {
            get { return _extents.Value; }
            set { _extents.Value = value; }
        }

        public Vector3 Normal
        {
            get { return _normal.Value; }
            set { _normal.Value = value; }
        }

        public Vector3 Binormal
        {
            get { return _binormal.Value; }
            set { _binormal.Value = value; }
        }

        public Matrix PlaneMatrix
        {
            get
            {
                var tangent = Vector3.Normalize(Vector3.Cross(Normal, Binormal));
                var leftBottom = Position - Binormal * Extents.X + tangent * Extents.Y;

                var right = Binormal;
                var forward = -tangent;
                var up = Normal;

                Matrix m = new Matrix(
                    right.X, right.Y, right.Z, 0,
                    forward.X, forward.Y, forward.Z, 0,
                    up.X, up.Y, up.Z, 0,
                    leftBottom.X, leftBottom.Y, leftBottom.Z, 1
                );

                //This converts a 3D point into a point on the XY plane, where 0->1 is the range of the plane from left to right
                var size = Extents * 2;
                return Matrix.Invert(m);//*Matrix.CreateScale(new Vector3(1 / size.X, 1 / size.Y, 0));
            }
        }

        public override void CreateProperties(Entity.ConstructionContext context)
        {
            _texture = context.CreateProperty<Texture2D>(TEXTURE_NAME);
            _position = context.CreateProperty<Vector3>(POSITION_NAME);
            _range = context.CreateProperty<float>(RANGE_NAME);
            _extents = context.CreateProperty<Vector2>(EXTENTS_NAME);
            _normal = context.CreateProperty<Vector3>(NORMAL_NAME);
            _binormal = context.CreateProperty<Vector3>(BINORMAL_NAME);

            base.CreateProperties(context);
        }

        public override void Initialise(Collections.INamedDataProvider initialisationData)
        {
            base.Initialise(initialisationData);

            initialisationData.TryCopyValue(TEXTURE_NAME, _texture);
            initialisationData.TryCopyValue(POSITION_NAME, _position);
            initialisationData.TryCopyValue(RANGE_NAME, _range);
            initialisationData.TryCopyValue(EXTENTS_NAME, _extents);
            initialisationData.TryCopyValue(NORMAL_NAME, _normal);
            initialisationData.TryCopyValue(BINORMAL_NAME, _binormal);
        }
    }
}
