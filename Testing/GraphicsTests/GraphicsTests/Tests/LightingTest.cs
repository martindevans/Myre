using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Myre.Graphics;
using Microsoft.Xna.Framework.Graphics;
using Ninject;
using Myre.Entities;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework;
using Myre.Graphics.Geometry;
using System.IO;
using Myre.Graphics.Lighting;
using Microsoft.Xna.Framework.Input;
using Myre.Graphics.Deferred;

namespace GraphicsTests.Tests
{
    class LightingTest
    : TestScreen
    {
        class Phase
            : RendererComponent
        {
            private SpriteBatch batch;
            private bool drawGBuffer;
            private KeyboardState previousKeyboard;

            public Phase(GraphicsDevice device)
            {
                batch = new SpriteBatch(device);
            }

            //protected override void SpecifyResources(IList<Input> inputs, IList<RendererComponent.Resource> outputs, out RenderTargetInfo? outputTarget)
            //{
            //    inputs.Add(new Input() { Name = "gbuffer_depth" });
            //    inputs.Add(new Input() { Name = "gbuffer_normals" });
            //    inputs.Add(new Input() { Name = "gbuffer_diffuse" });
            //    inputs.Add(new Input() { Name = "lightbuffer" });
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
                context.DefineInput("gbuffer_normals");
                context.DefineInput("gbuffer_diffuse");
                context.DefineInput("lightbuffer");

                // define outputs
                context.DefineOutput("scene");
                
                base.Initialise(renderer, context);
            }

            public override void Draw(Renderer renderer)
            {
                KeyboardState keyboard = Keyboard.GetState();
                if (keyboard.IsKeyDown(Keys.Space) && previousKeyboard.IsKeyUp(Keys.Space))
                    drawGBuffer = !drawGBuffer;
                previousKeyboard = keyboard;

                var metadata = renderer.Data;
                var resolution = renderer.Data.Get<Vector2>("resolution").Value;
                var targetInfo = new RenderTargetInfo() { Width = (int)resolution.X, Height = (int)resolution.Y, SurfaceFormat = SurfaceFormat.Rgba64 };
                var target = RenderTargetManager.GetTarget(renderer.Device, targetInfo);
                renderer.Device.SetRenderTarget(target);

                var depth = metadata.Get<Texture2D>("gbuffer_depth").Value;
                var normals = metadata.Get<Texture2D>("gbuffer_normals").Value;
                var diffuse = metadata.Get<Texture2D>("gbuffer_diffuse").Value;
                var light = metadata.Get<Texture2D>("lightbuffer").Value;

                //using (var stream = File.Create("lightbuffer.jpg"))
                //    light.SaveAsJpeg(stream, light.Width, light.Height);

                var halfWidth = (int)(resolution.X / 2);
                var halfHeight = (int)(resolution.Y / 2);

                batch.GraphicsDevice.Clear(Color.Black);
                batch.Begin(SpriteSortMode.Immediate, BlendState.Opaque);

                if (drawGBuffer)
                {
                    batch.GraphicsDevice.SamplerStates[0] = SamplerState.PointClamp;
                    batch.Draw(depth, new Rectangle(0, 0, halfWidth, halfHeight), Color.White);
                    batch.Draw(light, new Rectangle(halfWidth, halfHeight, halfWidth, halfHeight), Color.White);
                    batch.GraphicsDevice.SamplerStates[0] = SamplerState.LinearClamp;

                    batch.Draw(normals, new Rectangle(halfWidth, 0, halfWidth, halfHeight), Color.White);
                    batch.Draw(diffuse, new Rectangle(0, halfHeight, halfWidth, halfHeight), Color.White);
                }
                else
                {
                    batch.GraphicsDevice.SamplerStates[0] = SamplerState.PointClamp;
                    batch.Draw(light, new Rectangle(0, 0, (int)resolution.X, (int)resolution.Y), Color.White);
                    batch.GraphicsDevice.SamplerStates[0] = SamplerState.LinearClamp;
                }

                batch.End();

                Output("scene", target);
            }
        }


        private IKernel kernel;
        private ContentManager content;
        private GraphicsDevice device;
        private TestScene scene;

        public LightingTest(
            IKernel kernel,
            ContentManager content,
            GraphicsDevice device)
            : base("Lighting", kernel)
        {
            this.kernel = kernel;
            this.content = content;
            this.device = device;
        }

        protected override void BeginTransitionOn()
        {
            scene = kernel.Get<TestScene>();

            var renderer = scene.Scene.GetService<Renderer>();
            renderer.StartPlan()
                .Then<GeometryBufferComponent>()
                .Then<EdgeDetectComponent>()
                .Then<Ssao>()
                .Then<LightingComponent>()
                .Then<Phase>()
                //.Show("shadowmap")
                .Apply();

            base.OnShown();
        }

        public override void Update(GameTime gameTime)
        {
            scene.Update(gameTime);
            base.Update(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
            scene.Draw(gameTime);
            base.Draw(gameTime);
        }
    }
}
