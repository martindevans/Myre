using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Myre.Graphics;
using Myre.Graphics.Deferred;
using Ninject;
using System.IO;
using System.Numerics;

using Color = Microsoft.Xna.Framework.Color;
using GameTime = Microsoft.Xna.Framework.GameTime;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace GraphicsTests.Tests
{
    class GBufferTest
    : TestScreen
    {
        class Phase
            : RendererComponent
        {
            private readonly SpriteBatch _batch;

            public Phase(GraphicsDevice device)
            {
                _batch = new SpriteBatch(device);
            }

            //protected override void SpecifyResources(IList<Input> inputs, IList<RendererComponent.Resource> outputs, out RenderTargetInfo? outputTarget)
            //{
            //    inputs.Add(new Input() { Name = "gbuffer_depth" });
            //    inputs.Add(new Input() { Name = "gbuffer_depth_downsample" });
            //    inputs.Add(new Input() { Name = "gbuffer_normals" });
            //    inputs.Add(new Input() { Name = "gbuffer_diffuse" });
            //    outputs.Add(new Resource() { Name = "scene", IsLeftSet = true });

            //    outputTarget = new RenderTargetInfo();
            //}

            //protected override bool ValidateInput(RenderTargetInfo? previousRenderTarget)
            //{
            //    return true;
            //}

            public override void Initialise(Renderer renderer, ResourceContext context)
            {
                // define inputs
                context.DefineInput("gbuffer_depth");
                context.DefineInput("gbuffer_depth_downsample");
                context.DefineInput("gbuffer_normals");
                context.DefineInput("gbuffer_diffuse");

                // define outputs
                context.DefineOutput("scene");
                
                base.Initialise(renderer, context);
            }

            public override void Draw(Renderer renderer)
            {
                var metadata = renderer.Data;
                var resolution = renderer.Data.Get<Vector2>("resolution").Value;
                var targetInfo = new RenderTargetInfo((int) resolution.X, (int) resolution.Y, default(SurfaceFormat), default(DepthFormat), default(int), default(bool), default(RenderTargetUsage));
                var target = RenderTargetManager.GetTarget(renderer.Device, targetInfo);
                renderer.Device.SetRenderTarget(target);

                var depth = metadata.Get<Texture2D>("gbuffer_depth").Value;
                var normals = metadata.Get<Texture2D>("gbuffer_normals").Value;
                var diffuse = metadata.Get<Texture2D>("gbuffer_diffuse").Value;

                //Save(depth, "depth.jpg");
                //Save(normals, "normal.jpg");
                //Save(diffuse, "diffuse.jpg");

                var halfWidth = (int)(resolution.X / 2);
                var halfHeight = (int)(resolution.Y / 2);

                _batch.GraphicsDevice.Clear(Color.Black);
                _batch.Begin(SpriteSortMode.Immediate, BlendState.Opaque);

                _batch.GraphicsDevice.SamplerStates[0] = SamplerState.PointClamp;
                _batch.Draw(depth, new Rectangle(0, 0, halfWidth, halfHeight), Color.White);
                _batch.GraphicsDevice.SamplerStates[0] = SamplerState.LinearClamp;

                _batch.Draw(normals, new Rectangle(halfWidth, 0, halfWidth, halfHeight), Color.White);
                _batch.Draw(diffuse, new Rectangle(0, halfHeight, halfWidth, halfHeight), Color.White);
                _batch.End();

                Output("scene", target);
            }

            private void Save(Texture2D texture, string name)
            {
                using (Stream stream = File.Create(name))
                    texture.SaveAsJpeg(stream, texture.Width, texture.Height);
            }
        }


        private readonly IKernel _kernel;
        private TestScene _scene;

        public GBufferTest(
            IKernel kernel,
            ContentManager content,
            GraphicsDevice device)
            : base("Geometry Buffer", kernel)
        {
            _kernel = kernel;
        }

        protected override void BeginTransitionOn()
        {
            _scene = _kernel.Get<TestScene>();

            var renderer = _scene.Scene.GetService<Renderer>();
            renderer.StartPlan()
                .Then<GeometryBufferComponent>()
                .Then<Phase>()
                .Apply();

            base.OnShown();
        }

        public override void Update(GameTime gameTime)
        {
            _scene.Update(gameTime);
            base.Update(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
            _scene.Draw(gameTime);
            base.Draw(gameTime);
        }
    }
}
