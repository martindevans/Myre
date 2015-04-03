using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Myre.Entities;
using Myre.Entities.Behaviours;
using Myre.Graphics.Deferred.Decals;

namespace Myre.Graphics.Geometry
{
    [DefaultManager(typeof(Decal.Manager))]
    public class Sprite
        : Behaviour
    {
        public static readonly TypedName<Texture2D> TextureName = new TypedName<Texture2D>("texture");
        public static readonly TypedName<Vector2> PositionName = new TypedName<Vector2>("position");
        public static readonly TypedName<Rectangle?> SourceRectangleName = new TypedName<Rectangle?>("source_rectangle");
        public static readonly TypedName<Color> ColorName = new TypedName<Color>("colour");
        public static readonly TypedName<float> RotationName = new TypedName<float>("rotation");
        public static readonly TypedName<Vector2> OriginName = new TypedName<Vector2>("origin");
        public static readonly TypedName<Vector2> ScaleName = new TypedName<Vector2>("scale");
        public static readonly TypedName<SpriteEffects> SpriteEffectsName = new TypedName<SpriteEffects>("sprite_effects");
        public static readonly TypedName<float> LayerDepthName = new TypedName<float>("layer_depth");

        private Property<Texture2D> _texture;
        public Texture2D Texture
        {
            get
            {
                return _texture.Value;
            }
            set
            {
                _texture.Value = value;
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

        private Property<Rectangle?> _sourceRectangle;
        public Rectangle? SourceRectangle
        {
            get
            {
                return _sourceRectangle.Value;
            }
            set
            {
                _sourceRectangle.Value = value;
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

        private Rectangle MaximumBounds
        {
            get
            {
                if (_texture.Value == null)
                    return new Rectangle(int.MaxValue, int.MaxValue, 0, 0);

                //Manhattan length of the diagonal (scaled)
                var diagonal = new Vector2(_texture.Value.Width / 2 + _texture.Value.Height / 2) * Scale;

                //Position of the bottom left
                var pos = _position.Value - diagonal;

                return new Rectangle(
                    //Position, rounded *down* (loss of 1)
                    (int)pos.X,
                    (int)pos.Y,

                    //Width, rounded *down* (loss of 1)
                    //Add on 2 to make up for the potential 2 lost
                    (int)(diagonal.X * 2) + 2,
                    (int)(diagonal.Y * 2) + 2
                );
            }
        }

        public override void CreateProperties(Entity.ConstructionContext context)
        {
            base.CreateProperties(context);

            _texture = context.CreateProperty(TextureName);
            _position = context.CreateProperty(PositionName);
            _sourceRectangle = context.CreateProperty(SourceRectangleName);
            _color = context.CreateProperty(ColorName);
            _rotation = context.CreateProperty(RotationName);
            _origin = context.CreateProperty(OriginName);
            _scale = context.CreateProperty(ScaleName);
            _spriteEffects = context.CreateProperty(SpriteEffectsName);
            _layerDepth = context.CreateProperty(LayerDepthName);
            _isInvisible = context.CreateProperty(ModelInstance.IsInvisibleName);
        }

        protected virtual void Prepare()
        {
        }

        private void Draw(SpriteBatch batch)
        {
            batch.Draw(Texture, Position, SourceRectangle, Color, Rotation, Origin, Scale, SpriteEffects, LayerDepth);
        }

        private bool IsInView(Viewport view)
        {
            return  view.Bounds.Intersects(MaximumBounds);
        }

        internal class Manager
            : BehaviourManager<Sprite>
        {
            public void Draw(Viewport view, SpriteBatch batch)
            {
                foreach (var sprite in Behaviours)
                {
                    sprite.Prepare();

                    if (!sprite.IsInvisible && sprite.IsInView(view))
                        sprite.Draw(batch);
                }
            }
        }
    }

    public class SpriteComponent
        : RendererComponent
    {
        private SpriteBatch _batch;
        private Sprite.Manager _sprites;

        public override void Initialise(Renderer renderer, ResourceContext context)
        {
            base.Initialise(renderer, context);

            _batch = new SpriteBatch(renderer.Device);
            _sprites = renderer.Scene.GetManager<Sprite.Manager>();
        }

        public override void Draw(Renderer renderer)
        {
            var viewport = renderer.Data.Get<Viewport>("viewport");

            _batch.Begin();
            _sprites.Draw(viewport.Value, _batch);
            _batch.End();
        }
    }
}
