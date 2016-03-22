using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Myre;
using Myre.Collections;
using Myre.Entities;
using Myre.Graphics;
using Myre.Graphics.Deferred;
using Myre.Graphics.Translucency;
using Ninject;

using Game = Microsoft.Xna.Framework.Game;
using GameTime = Microsoft.Xna.Framework.GameTime;

namespace GraphicsTests.Tests
{
    class GBufferTest2
        : TestScreen
    {
        private readonly IKernel _kernel;
        private TestScene _scene;
        private Renderer _renderer;

        public GBufferTest2(IKernel kernel, ContentManager content, GraphicsDevice device)
            : base("Geometry Buffer 2", kernel)
        {
            _kernel = kernel;
        }

        protected override void BeginTransitionOn()
        {
            _scene = new TestScene(_kernel, _kernel.Get<Game>(), _kernel.Get<ContentManager>(), _kernel.Get<GraphicsDevice>(), new TestScene.SceneConfiguration()
            {
                View = false
            });

            _renderer = _scene.Scene.GetService<Renderer>();
            _renderer.StartPlan()
                    .Then<GeometryBufferComponent>()
                    .Then<EdgeDetectComponent>()
                    .Then<Ssao>()
                    .Then<LightingComponent>()
                    .Then<RestoreDepthPhase>()
                    .Then<TranslucentComponent>()
                    .Then<ToneMapComponent>()
                    .Then<AntiAliasComponent>()
                    .Apply();

            var w = _renderer.Device.Viewport.Width / 2;
            var h = _renderer.Device.Viewport.Height / 2;
            AddCamera("gbuffer_depth", new Viewport { X = 0, Y = 0, Width = w, Height = h });
            AddCamera("gbuffer_normals", new Viewport { X = w, Y = 0, Width = w, Height = h });
            AddCamera("gbuffer_diffuse", new Viewport { X = 0, Y = h, Width = w, Height = h });
            AddCamera("lightbuffer", new Viewport { X = w, Y = h, Width = w, Height = h });

            base.OnShown();
        }

        private void AddCamera(string show, Viewport v)
        {
            var cameraDesc = _kernel.Get<EntityDescription>();
            cameraDesc.AddProperty(new TypedName<Camera>("camera"));
            cameraDesc.AddProperty(new TypedName<Viewport>("viewport"));
            cameraDesc.AddBehaviour<OutputPlanView>();

            var cameraEntity = cameraDesc.Create();
            cameraEntity.GetProperty(new TypedName<Camera>("camera")).Value = _scene.Camera;
            cameraEntity.GetProperty(new TypedName<Viewport>("viewport")).Value = v;

            NamedBoxCollection init = new NamedBoxCollection();
            init.Set(new TypedName<string>("output"), show);

            _scene.Scene.Add(cameraEntity, init);
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

        private class OutputPlanView
            : PlanView
        {
            private string _output;

            public override void Initialise(INamedDataProvider initialisationData)
            {
                base.Initialise(initialisationData);

                _output = initialisationData.GetValue(new TypedName<string>("output"), false);
            }

            protected override RenderPlan CreatePlan(Renderer renderer)
            {
                return renderer.Plan.Clone()
                    .Show(_output);
            }
        }
    }
}
