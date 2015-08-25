using System.Numerics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Myre;
using Myre.Entities;
using Myre.Extensions;
using Myre.Graphics;
using Ninject;
using Color = Microsoft.Xna.Framework.Color;
using GameTime = Microsoft.Xna.Framework.GameTime;

namespace GraphicsTests.Tests
{
    class ClearPhase
        : RendererComponent
    {
        public Color Colour;

        public override void Initialise(Renderer renderer, ResourceContext context)
        {
            // define input
            context.DefineInput("scene");

            // define output
            context.DefineOutput("scene");

            base.Initialise(renderer, context);
        }

        public override void Draw(Renderer renderer)
        {
            var resolution = renderer.Data.Get<Vector2>("resolution").Value;
            var targetInfo = new RenderTargetInfo((int) resolution.X, (int) resolution.Y, SurfaceFormat.Color, DepthFormat.None, 4, default(bool), default(RenderTargetUsage));

            var target = RenderTargetManager.GetTarget(renderer.Device, targetInfo);
            renderer.Device.SetRenderTarget(target);
            renderer.Device.Clear(Colour);

            Output("scene", target);
        }
    }

    class RenderPhaseTest
        : TestScreen
    {
        class Phase
            : RendererComponent
        {
            private readonly SpriteBatch _batch;
            public SpriteFont Font;

            public Phase(GraphicsDevice device)
            {
                _batch = new SpriteBatch(device);
            }

            public override void Initialise(Renderer renderer, ResourceContext context)
            {
                // define output
                context.DefineOutput("scene");

                base.Initialise(renderer, context);
            }

            public override void Draw(Renderer renderer)
            {
                var resolution = renderer.Data.Get<Vector2>("resolution").Value;
                var targetInfo = new RenderTargetInfo((int) resolution.X, (int) resolution.Y, default(SurfaceFormat), default(DepthFormat), default(int), default(bool), default(RenderTargetUsage));
                var target = RenderTargetManager.GetTarget(renderer.Device, targetInfo);
                renderer.Device.SetRenderTarget(target);

                _batch.Begin();
                _batch.DrawString(Font, "This is being drawn by a RenderPhase!", new Vector2(640, 360).ToXNA(), Color.White);
                _batch.End();

                Output("scene", target);
            }
        }


        private readonly IKernel _kernel;
        private readonly ContentManager _content;
        private readonly GraphicsDevice _device;
        private Scene _scene;

        public RenderPhaseTest(
            IKernel kernel,
            ContentManager content,
            GraphicsDevice device)
            : base("Render Phase", kernel)
        {
            _kernel = kernel;
            _content = content;
            _device = device;
        }

        protected override void BeginTransitionOn()
        {
            _scene = new Scene(_kernel);
            
            var camera = new EntityDescription(_kernel);
            camera.AddProperty(new TypedName<Camera>("camera"));
            camera.AddProperty(new TypedName<Viewport>("viewport"));
            camera.AddBehaviour<View>();
            var cameraEntity = camera.Create();
            cameraEntity.GetProperty(new TypedName<Camera>("camera")).Value = new Camera();
            cameraEntity.GetProperty(new TypedName<Viewport>("viewport")).Value = new Viewport() { Width = 1280, Height = 720 };
            _scene.Add(cameraEntity);

            var renderer = _scene.GetService<Renderer>();
            renderer.StartPlan()
                .Then(new Phase(_device) { Font = _content.Load<SpriteFont>("Consolas") })
                .Then(new ClearPhase() { Colour = Color.Black })
                .Apply();

            base.OnShown();
        }

        public override void Update(GameTime gameTime)
        {
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
