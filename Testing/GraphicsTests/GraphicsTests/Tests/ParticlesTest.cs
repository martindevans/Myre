using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Myre.Entities;
using Myre.Graphics;
using Myre.Graphics.Translucency;
using Myre.Graphics.Translucency.Particles;
using Myre.Graphics.Translucency.Particles.Initialisers.AngularVelocity;
using Myre.Graphics.Translucency.Particles.Initialisers.Colour;
using Myre.Graphics.Translucency.Particles.Initialisers.Lifetime;
using Myre.Graphics.Translucency.Particles.Initialisers.Position;
using Myre.Graphics.Translucency.Particles.Initialisers.Size;
using Myre.Graphics.Translucency.Particles.Initialisers.Velocity;
using Ninject;

namespace GraphicsTests.Tests
{
    class ParticlesTest
        : TestScreen
    {
        private readonly IKernel _kernel;
        private Scene _scene;
        private readonly ContentManager _content;
        private readonly GraphicsDevice _device;
        private EntityParticleEmitter _emitter;
        private Camera _camera;

        private KeyboardState _keyboardState;
        private bool _spin = true;
        private float _rotation = 0;

        public ParticlesTest(IKernel kernel, ContentManager content, GraphicsDevice device)
            : base("Particles", kernel)
        {
            _kernel = kernel;
            _content = content;
            _device = device;
        }

        protected override void BeginTransitionOn()
        {
            if (_scene != null)
                return;

            _scene = _kernel.Get<Scene>();

            var renderer = _scene.GetService<Renderer>();
            renderer.StartPlan()
                    .Then(new CreateTargetComponent(new RenderTargetInfo(0, 0, default(SurfaceFormat), DepthFormat.Depth24Stencil8, default(int), default(bool), default(RenderTargetUsage))))
                    .Then<TranslucentComponent>()
                    .Apply();

            var cameraPosition = new Vector3(0, 25, -200);

            _camera = new Camera
            {
                NearClip = 1,
                FarClip = 3000,
                View = Matrix.CreateLookAt(cameraPosition, new Vector3(0, 25, 0),
                Vector3.Up)
            };
            _camera.Projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(60), 16f / 9f, _camera.NearClip, _camera.FarClip);

            var cameraDescription = _kernel.Get<EntityDescription>();
            cameraDescription.AddProperty<Viewport>("viewport");
            cameraDescription.AddBehaviour<View>();
            var cameraEntity = cameraDescription.Create();
            cameraEntity.GetProperty<Camera>("camera").Value = _camera;
            cameraEntity.GetProperty<Viewport>("viewport").Value = new Viewport() { Width = _device.PresentationParameters.BackBufferWidth, Height = _device.PresentationParameters.BackBufferHeight };
            _scene.Add(cameraEntity);

            var particleEntityDesc = _kernel.Get<EntityDescription>();
            particleEntityDesc.AddProperty<Vector3>("position");
            particleEntityDesc.AddBehaviour<EntityParticleEmitter>();
            var entity = particleEntityDesc.Create();
            _emitter = entity.GetBehaviour<EntityParticleEmitter>();
            entity.GetProperty<Vector3>("position").Value = Vector3.Zero;
            _scene.Add(entity);

            var white = new Texture2D(_device, 1, 1);
            white.SetData(new Color[] { Color.White });

            _emitter.AddInitialiser(new Ellipsoid(new Vector3(10, 100, 100), 75));
            _emitter.AddInitialiser(new RandomVelocity(new Vector3(20, 20, 20)));
            _emitter.AddInitialiser(new RandomAngularVelocity(-MathHelper.PiOver4, MathHelper.PiOver4));
            _emitter.AddInitialiser(new RandomSize(30, 40));
            _emitter.AddInitialiser(new RandomStartColour(Color.Red, Color.White));
            _emitter.AddInitialiser(new RandomEndColour(Color.White, Color.Blue));
            _emitter.AddInitialiser(new RandomLifetime(0.7f, 1f));

            _emitter.BlendState = BlendState.Additive;
            _emitter.Enabled = true;
            _emitter.EndLinearVelocity = 0.25f;
            _emitter.EndScale = 1.75f;
            _emitter.Gravity = new Vector3(0, 0, 0);
            _emitter.Lifetime = 5f;
            _emitter.Texture = _content.Load<Texture2D>("fire");
            _emitter.EmitPerSecond = 1000;
            _emitter.Capacity = (int)(_emitter.Lifetime * _emitter.EmitPerSecond + 1);
            _emitter.VelocityBleedThrough = 0;
            
            base.OnShown();
        }

        public override void Update(GameTime gameTime)
        {
            if (Keyboard.GetState().IsKeyDown(Keys.Space) && !_keyboardState.IsKeyDown(Keys.Space))
                _spin = !_spin;
            _keyboardState = Keyboard.GetState();

            if (_spin)
                _rotation += (float)gameTime.ElapsedGameTime.TotalSeconds;
            _camera.View = Matrix.CreateLookAt(new Vector3((float)Math.Sin(_rotation) * 300, 0, (float)Math.Cos(_rotation) * 300), new Vector3(0, 0, 0), Vector3.Up);

            _scene.Update((float)gameTime.ElapsedGameTime.TotalSeconds);
            base.Update(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
            _scene.Draw();
            base.Draw(gameTime);
        }
    }
}
