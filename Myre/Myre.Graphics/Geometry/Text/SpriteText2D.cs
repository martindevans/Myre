using Microsoft.Xna.Framework.Graphics;
using Myre.Collections;
using Myre.Entities;
using Myre.Entities.Behaviours;
using Myre.Entities.Extensions;
using System;
using System.Numerics;
using Myre.Extensions;
using Color = Microsoft.Xna.Framework.Color;

namespace Myre.Graphics.Geometry.Text
{
    [DefaultManager(typeof(Manager))]
    public class SpriteText2D
        : Behaviour
    {
        public static readonly TypedName<SpriteFont> FontName = new TypedName<SpriteFont>("font");
        public static readonly TypedName<Vector2> PositionName = new TypedName<Vector2>("position");
        public static readonly TypedName<string> StringName = new TypedName<string>("string");
        public static readonly TypedName<Color> ColorName = new TypedName<Color>("colour");
        public static readonly TypedName<float> RotationName = new TypedName<float>("rotation");
        public static readonly TypedName<Vector2> OriginName = new TypedName<Vector2>("origin");
        public static readonly TypedName<Vector2> ScaleName = new TypedName<Vector2>("scale");
        public static readonly TypedName<SpriteEffects> SpriteEffectsName = new TypedName<SpriteEffects>("sprite_effects");
        public static readonly TypedName<float> LayerDepthName = new TypedName<float>("layer_depth");

        private Property<SpriteFont> _font;
        public SpriteFont Font
        {
            get
            {
                return _font.Value;
            }
            set
            {
                _font.Value = value;
            }
        }

        private Property<Vector2> _position;
        public Vector2 Position
        {
            get
            {
                return _position.Value;
            }
            set
            {
                _position.Value = value;
            }
        }

        private Property<string> _string;
        public string String
        {
            get
            {
                return _string.Value;
            }
            set
            {
                _string.Value = value;
            }
        }

        private Property<Color> _color;
        public Color Color
        {
            get
            {
                return _color.Value;
            }
            set
            {
                _color.Value = value;
            }
        }

        private Property<float> _rotation;
        public float Rotation
        {
            get
            {
                return _rotation.Value;
            }
            set
            {
                _rotation.Value = value;
            }
        }

        private Property<Vector2> _origin;
        public Vector2 Origin
        {
            get
            {
                return _origin.Value;
            }
            set
            {
                _origin.Value = value;
            }
        }

        private Property<Vector2> _scale;
        public Vector2 Scale
        {
            get
            {
                return _scale.Value;
            }
            set
            {
                _scale.Value = value;
            }
        }

        private Property<SpriteEffects> _spriteEffects;
        public SpriteEffects SpriteEffects
        {
            get
            {
                return _spriteEffects.Value;
            }
            set
            {
                _spriteEffects.Value = value;
            }
        }

        private Property<float> _layerDepth;
        public float LayerDepth
        {
            get
            {
                return _layerDepth.Value;
            }
            set
            {
                _layerDepth.Value = value;
            }
        }

        private Property<bool> _isInvisible;
        public bool IsInvisible
        {
            get
            {
                return _isInvisible.Value;
            }
            set
            {
                _isInvisible.Value = value;
            }
        }

        public override void CreateProperties(Entity.ConstructionContext context)
        {
            base.CreateProperties(context);

            _font = context.CreateProperty(FontName);
            _position = context.CreateProperty(PositionName);
            _string = context.CreateProperty(StringName);
            _color = context.CreateProperty(ColorName);
            _rotation = context.CreateProperty(RotationName);
            _origin = context.CreateProperty(OriginName);
            _scale = context.CreateProperty(ScaleName);
            _spriteEffects = context.CreateProperty(SpriteEffectsName);
            _layerDepth = context.CreateProperty(LayerDepthName);
            _isInvisible = context.CreateProperty(ModelInstance.IsInvisibleName);
        }

        public override void Initialise(INamedDataProvider initialisationData)
        {
            base.Initialise(initialisationData);

            initialisationData.TryCopyValue(this, FontName, _font);
            initialisationData.TryCopyValue(this, PositionName, _position);
            initialisationData.TryCopyValue(this, StringName, _string);
            initialisationData.TryCopyValue(this, ColorName, _color);
            initialisationData.TryCopyValue(this, RotationName, _rotation);
            initialisationData.TryCopyValue(this, OriginName, _origin);
            initialisationData.TryCopyValue(this, ScaleName, _scale);
            initialisationData.TryCopyValue(this, SpriteEffectsName, _spriteEffects);
            initialisationData.TryCopyValue(this, LayerDepthName, _layerDepth);
            initialisationData.TryCopyValue(this, ModelInstance.IsInvisibleName, _isInvisible);

            _font.PropertySet += (_, __, ___) => RecalculateBounds();
            _string.PropertySet += (_, __, ___) => RecalculateBounds();
            RecalculateBounds();
        }

        private Vector2 _stringBounds;
        private void RecalculateBounds()
        {
            _stringBounds = _font.Value.MeasureString(_string.Value).FromXNA();
        }

        private bool Prepare(View view)
        {
            throw new NotImplementedException();
        }

        private bool IsInView(View view)
        {
            return true;
        }

        private void Draw(SpriteBatch batch)
        {
            batch.DrawString(Font, String, Position.ToXNA(), Color, Rotation, Origin.ToXNA(), Scale.ToXNA(), SpriteEffects, LayerDepth);
        }

        internal class Manager
            : BehaviourManager<SpriteText2D>
        {
            public void Draw(View view, SpriteBatch batch)
            {
                foreach (var sprite in Behaviours)
                {
                    if (!sprite.Prepare(view))
                        continue;

                    if (!sprite.IsInvisible && sprite.IsInView(view))
                        sprite.Draw(batch);
                }
            }
        }
    }
}
