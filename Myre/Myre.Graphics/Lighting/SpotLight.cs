using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Myre.Entities;
using Myre.Entities.Behaviours;

namespace Myre.Graphics.Lighting
{
    public class SpotLight
        : Behaviour
    {
        private Property<Vector3> _colour;
        private Property<Vector3> _position;
        private Property<Vector3> _direction;
        private Property<float> _angle;
        private Property<float> _range;
        private Property<Texture2D> _mask;
        private Property<int> _shadowResolution;
        private Property<bool> _active;

        public bool Active
        {
            get { return _active.Value; }
            set { _active.Value = value; }
        }

        public Vector3 Colour
        {
            get { return _colour.Value; }
            set { _colour.Value = value; }
        }

        public Vector3 Position
        {
            get { return _position.Value; }
            set { _position.Value = value; }
        }

        public Vector3 Direction
        {
            get { return _direction.Value; }
            set { _direction.Value = Vector3.Normalize(value); }
        }

        public float Angle
        {
            get { return _angle.Value; }
            set { _angle.Value = value; }
        }

        public float Range
        {
            get { return _range.Value; }
            set { _range.Value = value; }
        }

        public Texture2D Mask
        {
            get { return _mask.Value; }
            set { _mask.Value = value; }
        }

        public int ShadowResolution
        {
            get { return _shadowResolution.Value; }
            set { _shadowResolution.Value = value; }
        }

        public override void CreateProperties(Entity.ConstructionContext context)
        {
            _colour = context.CreateProperty<Vector3>("colour");
            _position = context.CreateProperty<Vector3>("position");
            _direction = context.CreateProperty<Vector3>("direction");
            _angle = context.CreateProperty<float>("angle");
            _range = context.CreateProperty<float>("range");
            _mask = context.CreateProperty<Texture2D>("mask");
            _shadowResolution = context.CreateProperty<int>("shadow_resolution");
            _active = context.CreateProperty<bool>("spotlight_active", true);

            base.CreateProperties(context);
        }
    }
}
