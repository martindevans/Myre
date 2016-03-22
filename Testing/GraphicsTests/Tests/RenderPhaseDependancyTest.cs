using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Myre;
using Myre.Entities;
using Myre.Extensions;
using Myre.Graphics;
using Ninject;
using System.Numerics;

using Color = Microsoft.Xna.Framework.Color;
using GameTime = Microsoft.Xna.Framework.GameTime;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace GraphicsTests.Tests
{
    class RenderPhaseDependancyTest
        : TestScreen
    {
        static SpriteBatch _spriteBatch;
        static SpriteFont _font;

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
                var target = RenderTargetManager.GetTarget(renderer.Device, new RenderTargetInfo(50, 50, default(SurfaceFormat), default(DepthFormat), default(int), default(bool), default(RenderTargetUsage)));
                renderer.Device.SetRenderTarget(target);
                renderer.Device.Clear(Color.White);
                _spriteBatch.Begin();
                _spriteBatch.DrawString(_font, "A", Vector2.Zero.ToXNA(), Color.Black);
                _spriteBatch.End();

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
                var target = RenderTargetManager.GetTarget(renderer.Device, new RenderTargetInfo(50, 50, default(SurfaceFormat), default(DepthFormat), default(int), default(bool), default(RenderTargetUsage)));
                renderer.Device.SetRenderTarget(target);
                _spriteBatch.Begin();
                _spriteBatch.DrawString(_font, "B", Vector2.Zero.ToXNA(), Color.White);
                _spriteBatch.End();

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
                var a = metadata.GetValue(new TypedName<Texture2D>("a"));
                var b = metadata.GetValue(new TypedName<Texture2D>("b"));

                _spriteBatch.Begin();
                _spriteBatch.Draw(a, new Rectangle(0, 0, 50, 50), Color.White);
                _spriteBatch.Draw(b, new Rectangle(50, 0, 50, 50), Color.White);
                _spriteBatch.End();

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
                var c = metadata.GetValue(new TypedName<Texture2D>("c"));

                _spriteBatch.Begin();
                _spriteBatch.Draw(c, new Rectangle(590, 335, 100, 50), Color.White);
                _spriteBatch.End();

                Output("d", target);
            }
        }


        private readonly IKernel _kernel;
        private readonly ContentManager _content;
        private readonly GraphicsDevice _device;
        private Scene _scene;

        public RenderPhaseDependancyTest(
            IKernel kernel,
            ContentManager content,
            GraphicsDevice device)
            : base("Render Phase Dependancies", kernel)
        {
            _kernel = kernel;
            _content = content;
            _device = device;
        }

        protected override void BeginTransitionOn()
        {
            _spriteBatch = new SpriteBatch(_device);
            _font = _content.Load<SpriteFont>("Consolas");

            _scene = new Scene(_kernel);
            
            var camera = new EntityDescription(_kernel);
            camera.AddProperty(new TypedName<Camera>("camera"));
            camera.AddProperty(new TypedName<Viewport>("viewport"));
            camera.AddBehaviour<View>();
            var cameraEntity = camera.Create();
            cameraEntity.GetProperty(new TypedName<Camera>("camera")).Value = new Camera();
            cameraEntity.GetProperty(new TypedName<Viewport>("viewport")).Value = new Viewport() { Height = 1920, Width = 1080 };
            _scene.Add(cameraEntity);

            var renderer = _scene.GetService<Renderer>();
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
