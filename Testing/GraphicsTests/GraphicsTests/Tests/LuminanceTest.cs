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
using Myre.UI.Gestures;
using Myre.Graphics.Particles;
using Myre.Graphics.Deferred;

namespace GraphicsTests.Tests
{
    class LuminanceTest
        : TestScreen
    {
        class Phase
            : RendererComponent
        {
            private SpriteBatch batch;
            private bool drawScene = true;
            private ToneMapComponent toneMap;

            public Phase(LuminanceTest test, GraphicsDevice device, ToneMapComponent toneMap)
            {
                batch = new SpriteBatch(device);
                this.toneMap = toneMap;

                test.UI.Root.Gestures.Bind((g, t, d) => { drawScene = !drawScene; }, new KeyPressed(Keys.Space));
            }

            //protected override void SpecifyResources(IList<Input> inputs, IList<RendererComponent.Resource> outputs, out RenderTargetInfo? outputTarget)
            //{
            //    inputs.Add(new Input() { Name = "tonemapped" });
            //    inputs.Add(new Input() { Name = "luminancemap" });
            //    outputs.Add(new Resource() { Name = "scene", IsLeftSet = true });
            //    outputTarget = new RenderTargetInfo() { SurfaceFormat = SurfaceFormat.Rgba64, DepthFormat = DepthFormat.Depth24Stencil8 };
            //}

            //protected override bool ValidateInput(RenderTargetInfo? previousRenderTarget)
            //{
            //    return true;
            //}

            public override void Initialise(Renderer renderer, ResourceContext context)
            {
                // define inputs
                context.DefineInput("tonemapped");
                //context.DefineInput("luminancemap");

                // define outputs
                context.DefineOutput("scene", surfaceFormat: SurfaceFormat.Rgba64, depthFormat: DepthFormat.Depth24Stencil8);
                
                base.Initialise(renderer, context);
            }

            public override void Draw(Renderer renderer)
            {
                var metadata = renderer.Data;
                var resolution = renderer.Data.Get<Vector2>("resolution").Value;
                var targetInfo = new RenderTargetInfo() { Width = (int)resolution.X, Height = (int)resolution.Y, SurfaceFormat = SurfaceFormat.Rgba64, DepthFormat = DepthFormat.Depth24Stencil8 };
                var target = RenderTargetManager.GetTarget(renderer.Device, targetInfo);
                renderer.Device.SetRenderTarget(target);

                var light = metadata.Get<Texture2D>("tonemapped").Value;
                var luminance = metadata.Get<Texture2D>("luminancemap").Value;

                //using (var stream = File.Create("luminance.jpg"))
                //    light.SaveAsJpeg(stream, light.Width, light.Height);

                var width = (int)resolution.X;
                var height = (int)resolution.Y;

                batch.GraphicsDevice.Clear(Color.Black);
                batch.Begin(SpriteSortMode.Immediate, BlendState.Opaque);

                if (drawScene)
                {
                    batch.GraphicsDevice.SamplerStates[0] = SamplerState.PointClamp;
                    batch.Draw(light, new Rectangle(0, 0, width, height), Color.White);
                    //batch.Draw(luminance, new Rectangle(50, height - (height / 5) - 50, height / 5, height / 5), Color.White);
                    //batch.Draw(toneMap.AdaptedLuminance, new Rectangle(50 + 20 + (height / 5), height - (height / 5) - 50, height / 5, height / 5), Color.White);
                    batch.GraphicsDevice.SamplerStates[0] = SamplerState.LinearClamp;
                }
                else
                {
                    batch.GraphicsDevice.SamplerStates[0] = SamplerState.PointClamp;
                    batch.Draw(luminance, new Rectangle(0, 0, width, height), Color.White);
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
        private Entity light;

        public LuminanceTest(
            IKernel kernel,
            ContentManager content,
            GraphicsDevice device)
            : base("Luminance", kernel)
        {
            this.kernel = kernel;
            this.content = content;
            this.device = device;

            UI.Root.Gestures.Bind((g, t, d) => light.GetProperty<Vector3>("colour").Value = new Vector3(5),
                new KeyPressed(Keys.L));

            UI.Root.Gestures.Bind((g, t, d) => light.GetProperty<Vector3>("colour").Value = Vector3.Zero,
                new KeyReleased(Keys.L));
        }

        protected override void BeginTransitionOn()
        {
            scene = kernel.Get<TestScene>();

            //var sun = kernel.Get<EntityDescription>();
            //sun.AddProperty<Vector3>("direction", Vector3.Down);
            //sun.AddProperty<Vector3>("colour", Vector3.One);
            //sun.AddProperty<int>("shadowresolution", 1024);
            //sun.AddBehaviour<SunLight>();
            //light = sun.Create();
            //scene.Scene.Add(light);

            var toneMap = kernel.Get<ToneMapComponent>();
            var renderer = scene.Scene.GetService<Renderer>();
            renderer.StartPlan()
                .Then<GeometryBufferComponent>()
                //.Then<EdgeDetectComponent>()
                .Then<Ssao>()
                .Then<LightingComponent>()
                .Then(toneMap)
                .Then(new Phase(this, device, toneMap))
                //.Then<RestoreDepthPhase>()
                .Then<ParticleComponent>()
                //.Then<AntiAliasComponent>()
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
