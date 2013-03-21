using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ninject;
using Myre.Graphics.Materials;
using Microsoft.Xna.Framework.Content;
using Myre.Graphics;
using Myre.Collections;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System.Diagnostics;
using System.Xml;
using Myre.Entities;

namespace GraphicsTests
{
    class ClearPhase
        : RendererComponent
    {
        public Color Colour;

        //protected override void SpecifyResources(IList<Input> inputs, IList<RendererComponent.Resource> outputs, out RenderTargetInfo? outputTarget)
        //{
        //    outputs.Add(new Resource() { Name = "scene", IsLeftSet = true });
        //    outputTarget = new RenderTargetInfo();
        //}

        //protected override bool ValidateInput(RenderTargetInfo? previousRenderTarget)
        //{
        //    return true;
        //}

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
            private SpriteBatch batch;
            public SpriteFont Font;

            public Phase(GraphicsDevice device)
            {
                batch = new SpriteBatch(device);
            }

            //protected override void SpecifyResources(IList<Input> inputs, IList<RendererComponent.Resource> outputs, out RenderTargetInfo? outputTarget)
            //{
            //    outputs.Add(new Resource() { Name = "scene", IsLeftSet = true });
            //    outputTarget = new RenderTargetInfo();
            //}

            //protected override bool ValidateInput(RenderTargetInfo? previousRenderTarget)
            //{
            //    return true;
            //}

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

                batch.Begin();
                batch.DrawString(Font, "This is being drawn by a RenderPhase!", new Vector2(640, 360), Color.White);
                batch.End();

                Output("scene", target);
            }
        }


        private IKernel kernel;
        private ContentManager content;
        private GraphicsDevice device;
        private Scene scene;

        public RenderPhaseTest(
            IKernel kernel,
            ContentManager content,
            GraphicsDevice device)
            : base("Render Phase", kernel)
        {
            this.kernel = kernel;
            this.content = content;
            this.device = device;
        }

        protected override void BeginTransitionOn()
        {
            scene = new Scene(kernel);
            
            var camera = new EntityDescription(kernel);
            camera.AddProperty<Camera>("camera");
            camera.AddProperty<Viewport>("viewport");
            camera.AddBehaviour<View>();
            var cameraEntity = camera.Create();
            cameraEntity.GetProperty<Camera>("camera").Value = new Camera();
            cameraEntity.GetProperty<Viewport>("viewport").Value = new Viewport() { Width = 1280, Height = 720 };
            scene.Add(camera.Create());

            var renderer = scene.GetService<Renderer>();
            renderer.StartPlan()
                .Then(new ClearPhase() { Colour = Color.Black })
                .Then(new Phase(device) { Font = content.Load<SpriteFont>("Consolas") })
                .Apply();

            base.OnShown();
        }

        public override void Update(GameTime gameTime)
        {
            scene.Update((float)gameTime.ElapsedGameTime.TotalSeconds);
            base.Update(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
            scene.Draw();
            base.Draw(gameTime);
        }
    }
}
