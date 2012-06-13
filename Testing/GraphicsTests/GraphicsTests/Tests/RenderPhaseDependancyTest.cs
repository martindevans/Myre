using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Myre.Graphics;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Ninject;
using Microsoft.Xna.Framework.Content;
using Myre.Entities;

namespace GraphicsTests.Tests
{
    class RenderPhaseDependancyTest
        : TestScreen
    {
        static SpriteBatch spriteBatch;
        static SpriteFont font;

        class A
            : RendererComponent
        {
            //protected override void SpecifyResources(IList<Input> inputs, IList<RendererComponent.Resource> outputs, out RenderTargetInfo? outputTarget)
            //{
            //    outputs.Add(new Resource() { Name = "a", IsLeftSet = true });

            //    outputTarget = new RenderTargetInfo();
            //}

            //protected override bool ValidateInput(RenderTargetInfo? previousRenderTarget)
            //{
            //    return true;
            //}

            public override void Initialise(Renderer renderer, ResourceContext context)
            {
                // define outputs
                context.DefineOutput("a");
                
                base.Initialise(renderer, context);
            }

            public override void Draw(Renderer renderer)
            {
                var target = RenderTargetManager.GetTarget(renderer.Device, new RenderTargetInfo() { Height = 50, Width = 50 });
                renderer.Device.SetRenderTarget(target);
                renderer.Device.Clear(Color.White);
                spriteBatch.Begin();
                spriteBatch.DrawString(font, "A", Vector2.Zero, Color.Black);
                spriteBatch.End();

                Output("a", target);
            }
        }

        class B
            : RendererComponent
        {
            //protected override void SpecifyResources(IList<Input> inputs, IList<RendererComponent.Resource> outputs, out RenderTargetInfo? outputTarget)
            //{
            //    outputs.Add(new Resource() { Name = "b", IsLeftSet = true });

            //    outputTarget = new RenderTargetInfo();
            //}

            //protected override bool ValidateInput(RenderTargetInfo? previousRenderTarget)
            //{
            //    return true;
            //}

            public override void Initialise(Renderer renderer, ResourceContext context)
            {
                // define outputs
                context.DefineOutput("b");
                
                base.Initialise(renderer, context);
            }

            public override void Draw(Renderer renderer)
            {
                var target = RenderTargetManager.GetTarget(renderer.Device, new RenderTargetInfo() { Height = 50, Width = 50 });
                renderer.Device.SetRenderTarget(target);
                spriteBatch.Begin();
                spriteBatch.DrawString(font, "B", Vector2.Zero, Color.White);
                spriteBatch.End();

                Output("b", target);
            }
        }

        class C
            : RendererComponent
        {
            //protected override void SpecifyResources(IList<Input> inputs, IList<RendererComponent.Resource> outputs, out RenderTargetInfo? outputTarget)
            //{
            //    inputs.Add(new Input() { Name = "a" });
            //    inputs.Add(new Input() { Name = "b" });
            //    outputs.Add(new Resource() { Name = "c", IsLeftSet = true });

            //    outputTarget = new RenderTargetInfo();
            //}

            //protected override bool ValidateInput(RenderTargetInfo? previousRenderTarget)
            //{
            //    return true;
            //}

            public override void Initialise(Renderer renderer, ResourceContext context)
            {
                // define inputs
                context.DefineInput("a");
                context.DefineInput("b");

                // define outputs
                context.DefineOutput("c");
                
                base.Initialise(renderer, context);
            }

            public override void Draw(Renderer renderer)
            {
                var target = RenderTargetManager.GetTarget(renderer.Device, 50, 100);
                renderer.Device.SetRenderTarget(target);

                var metadata = renderer.Data;
                var a = metadata.Get<Texture2D>("a").Value;
                var b = metadata.Get<Texture2D>("b").Value;

                spriteBatch.Begin();
                spriteBatch.Draw(a, new Rectangle(0, 0, 50, 50), Color.White);
                spriteBatch.Draw(b, new Rectangle(50, 0, 50, 50), Color.White);
                spriteBatch.End();

                Output("c", target);
            }
        }

        class D
            : RendererComponent
        {
            //protected override void SpecifyResources(IList<Input> inputs, IList<RendererComponent.Resource> outputs, out RenderTargetInfo? outputTarget)
            //{
            //    inputs.Add(new Input() { Name = "c" });
            //    outputs.Add(new Resource() { Name = "d", IsLeftSet = true });

            //    outputTarget = new RenderTargetInfo();
            //}

            //protected override bool ValidateInput(RenderTargetInfo? previousRenderTarget)
            //{
            //    return true;
            //}

            public override void Initialise(Renderer renderer, ResourceContext context)
            {
                // define inputs
                context.DefineInput("c");

                // define outputs
                context.DefineOutput("d");
                
                base.Initialise(renderer, context);
            }

            public override void Draw(Renderer renderer)
            {
                var target = RenderTargetManager.GetTarget(renderer.Device, 1280, 720);
                renderer.Device.SetRenderTarget(target);

                var metadata = renderer.Data;
                var c = metadata.Get<Texture2D>("c").Value;

                spriteBatch.Begin();
                spriteBatch.Draw(c, new Rectangle(590, 335, 100, 50), Color.White);
                spriteBatch.End();

                Output("d", target);
            }
        }


        private IKernel kernel;
        private ContentManager content;
        private GraphicsDevice device;
        private Scene scene;

        public RenderPhaseDependancyTest(
            IKernel kernel,
            ContentManager content,
            GraphicsDevice device)
            : base("Render Phase Dependancies", kernel)
        {
            this.kernel = kernel;
            this.content = content;
            this.device = device;
        }

        protected override void BeginTransitionOn()
        {
            spriteBatch = new SpriteBatch(device);
            font = content.Load<SpriteFont>("Consolas");

            scene = new Scene(kernel);
            
            var camera = new EntityDescription(kernel);
            camera.AddProperty<Camera>("camera");
            camera.AddProperty<Viewport>("viewport");
            camera.AddBehaviour<View>();
            var cameraEntity = camera.Create();
            cameraEntity.GetProperty<Camera>("camera").Value = new Camera();
            cameraEntity.GetProperty<Viewport>("viewport").Value = new Viewport() { Height = 1920, Width = 1080 };
            scene.Add(camera.Create());

            var renderer = scene.GetService<Renderer>();
            renderer.StartPlan()
                .Then<A>()
                .Then<B>()
                .Then<C>()
                .Then<D>()
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
