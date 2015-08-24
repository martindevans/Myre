using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Myre;
using Myre.Collections;
using Myre.Entities;
using Myre.Graphics;
using Myre.Graphics.Translucency;
using Myre.Graphics.Translucency.Particles;
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
            cameraDescription.AddProperty(new TypedName<Viewport>("viewport"));
            cameraDescription.AddBehaviour<View>();
            var cameraEntity = cameraDescription.Create();
            cameraEntity.GetProperty(new TypedName<Camera>("camera")).Value = _camera;
            cameraEntity.GetProperty(new TypedName<Viewport>("viewport")).Value = new Viewport() { Width = _device.PresentationParameters.BackBufferWidth, Height = _device.PresentationParameters.BackBufferHeight };
            _scene.Add(cameraEntity);

            var particleEntityDesc = _kernel.Get<EntityDescription>();
            particleEntityDesc.AddProperty(new TypedName<Vector3>("position"));
            particleEntityDesc.AddBehaviour<ParticleEmitter>();
            var entity = particleEntityDesc.Create();
            entity.GetProperty(new TypedName<Vector3>("position")).Value = Vector3.Zero;
            NamedBoxCollection initData = new NamedBoxCollection();
            initData.Set<ParticleEmitterDescription>("particlesystem", _content.Load<ParticleEmitterDescription>("Particles/TestEmitter1"));
            _scene.Add(entity, initData);
            
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
