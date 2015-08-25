using System.Numerics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Myre;
using Myre.Entities;
using Myre.Graphics;
using Myre.Graphics.Deferred;
using Myre.Graphics.Translucency;
using Myre.UI.Gestures;
using Ninject;

using Color = Microsoft.Xna.Framework.Color;
using GameTime = Microsoft.Xna.Framework.GameTime;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace GraphicsTests.Tests
{
    class LuminanceTest
        : TestScreen
    {
        class Phase
            : RendererComponent
        {
            private readonly SpriteBatch _batch;
            private bool _drawScene = true;

            public Phase(LuminanceTest test, GraphicsDevice device, ToneMapComponent toneMap)
            {
                _batch = new SpriteBatch(device);

                test.UI.Root.Gestures.Bind((g, t, d) => { _drawScene = !_drawScene; }, new KeyPressed(Keys.Space));
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
                var targetInfo = new RenderTargetInfo((int)resolution.X, (int)resolution.Y, SurfaceFormat.Rgba64, DepthFormat.Depth24Stencil8, default(int), default(bool), default(RenderTargetUsage));
                var target = RenderTargetManager.GetTarget(renderer.Device, targetInfo);
                renderer.Device.SetRenderTarget(target);

                var light = metadata.Get<Texture2D>("tonemapped").Value;
                var luminance = metadata.Get<Texture2D>("luminancemap").Value;

                //using (var stream = File.Create("luminance.jpg"))
                //    light.SaveAsJpeg(stream, light.Width, light.Height);

                var width = (int)resolution.X;
                var height = (int)resolution.Y;

                _batch.GraphicsDevice.Clear(Color.Black);
                _batch.Begin(SpriteSortMode.Immediate, BlendState.Opaque);

                if (_drawScene)
                {
                    _batch.GraphicsDevice.SamplerStates[0] = SamplerState.PointClamp;
                    _batch.Draw(light, new Rectangle(0, 0, width, height), Color.White);
                    //batch.Draw(luminance, new Rectangle(50, height - (height / 5) - 50, height / 5, height / 5), Color.White);
                    //batch.Draw(toneMap.AdaptedLuminance, new Rectangle(50 + 20 + (height / 5), height - (height / 5) - 50, height / 5, height / 5), Color.White);
                    _batch.GraphicsDevice.SamplerStates[0] = SamplerState.LinearClamp;
                }
                else
                {
                    _batch.GraphicsDevice.SamplerStates[0] = SamplerState.PointClamp;
                    _batch.Draw(luminance, new Rectangle(0, 0, width, height), Color.White);
                    _batch.GraphicsDevice.SamplerStates[0] = SamplerState.LinearClamp;
                }

                _batch.End();

                Output("scene", target);
            }
        }


        private readonly IKernel _kernel;
        private readonly GraphicsDevice _device;
        private TestScene _scene;
        private Entity light;

        public LuminanceTest(
            IKernel kernel,
            ContentManager content,
            GraphicsDevice device)
            : base("Luminance", kernel)
        {
            _kernel = kernel;
            _device = device;

            UI.Root.Gestures.Bind((g, t, d) => light.GetProperty(new TypedName<Vector3>("colour")).Value = new Vector3(5),
                new KeyPressed(Keys.L));

            UI.Root.Gestures.Bind((g, t, d) => light.GetProperty(new TypedName<Vector3>("colour")).Value = Vector3.Zero,
                new KeyReleased(Keys.L));
        }

        protected override void BeginTransitionOn()
        {
            _scene = _kernel.Get<TestScene>();

            //var sun = kernel.Get<EntityDescription>();
            //sun.AddProperty<Vector3>("direction", Vector3.Down);
            //sun.AddProperty<Vector3>("colour", Vector3.One);
            //sun.AddProperty<int>("shadowresolution", 1024);
            //sun.AddBehaviour<SunLight>();
            //light = sun.Create();
            //scene.Scene.Add(light);

            var toneMap = _kernel.Get<ToneMapComponent>();
            var renderer = _scene.Scene.GetService<Renderer>();
            renderer.StartPlan()
                .Then<GeometryBufferComponent>()
                //.Then<EdgeDetectComponent>()
                .Then<Ssao>()
                .Then<LightingComponent>()
                .Then(toneMap)
                .Then(new Phase(this, _device, toneMap))
                .Then<RestoreDepthPhase>()
                .Then<TranslucentComponent>()
                //.Then<AntiAliasComponent>()
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
