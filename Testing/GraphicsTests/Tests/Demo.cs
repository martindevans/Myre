using System.Numerics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Myre;
using Myre.Collections;
using Myre.Entities;
using Myre.Graphics;
using Myre.Graphics.Deferred;
using Myre.Graphics.Translucency;
using Myre.Graphics.Translucency.Particles;
using Ninject;

using GameTime = Microsoft.Xna.Framework.GameTime;

namespace GraphicsTests.Tests
{
    class Demo
        : TestScreen
    {
        private readonly IKernel _kernel;
        private readonly ContentManager _content;
        private TestScene _scene;
        private readonly TestGame _game;

        private Box<float> _ssaoIntensity;
        private RenderPlan _fullPlan;
        private RenderPlan _ssaoPlan;
        private RenderPlan _lightingPlan;
        private RenderPlan _edgeDetectPlan;
        private RenderPlan _normalPlan;
        private RenderPlan _depthPlan;
        private RenderPlan _diffusePlan;
        private RenderPlan _noAaPlan;

        public Demo(
            IKernel kernel,
            TestGame game,
            ContentManager content,
            GraphicsDevice device)
            : base("Demo", kernel)
        {
            _kernel = kernel;
            _content = content;
            _game = game;
        }

        protected override void BeginTransitionOn()
        {
            _scene = _kernel.Get<TestScene>();

            var particleEntityDesc = _scene.Scene.Kernel.Get<EntityDescription>();
            particleEntityDesc.AddProperty(new TypedName<Vector3>("position"));
            particleEntityDesc.AddBehaviour<ParticleEmitter>();
            var entity = particleEntityDesc.Create();
            entity.GetProperty(new TypedName<Vector3>("position")).Value = Vector3.Zero;
            NamedBoxCollection initData = new NamedBoxCollection();
            initData.Set<ParticleEmitterDescription>("particlesystem", _content.Load<ParticleEmitterDescription>("Particles/TestEmitter1"));
            _scene.Scene.Add(entity, initData);

            var renderer = _scene.Scene.GetService<Renderer>();

            _ssaoIntensity = renderer.Data.Get<float>("ssao_intensity");

            _fullPlan = renderer.StartPlan()
                               .Then<GeometryBufferComponent>()
                               .Then<EdgeDetectComponent>()
                               .Then<Ssao>()
                               .Then<LightingComponent>()
                               .Then<RestoreDepthPhase>()
                               //.Then<TranslucentComponent>()
                               .Then<ToneMapComponent>()
                               .Then<AntiAliasComponent>()
                               .Show("antialiased");
            _ssaoPlan = renderer.StartPlan()
                .Then<GeometryBufferComponent>()
                .Then<EdgeDetectComponent>()
                .Then<Ssao>()
                .Show("ssao");

            _lightingPlan = renderer.StartPlan()
                .Then<GeometryBufferComponent>()
                .Then<Ssao>()
                .Then<LightingComponent>()
                .Show("directlighting");

            _edgeDetectPlan = renderer.StartPlan()
                .Then<GeometryBufferComponent>()
                .Then<EdgeDetectComponent>()
                .Show("edges");

            _normalPlan = renderer.StartPlan()
                .Then<GeometryBufferComponent>()
                .Then(new AntiAliasComponent("gbuffer_normals"))
                .Show("antialiased");

            _depthPlan = renderer.StartPlan()
                .Then<GeometryBufferComponent>()
                .Show("gbuffer_depth");

            _diffusePlan = renderer.StartPlan()
                .Then<GeometryBufferComponent>()
                .Show("gbuffer_diffuse");

            _noAaPlan = renderer.StartPlan()
                               .Then<GeometryBufferComponent>()
                               .Then<EdgeDetectComponent>()
                               .Then<Ssao>()
                               .Then<LightingComponent>()
                               .Then<RestoreDepthPhase>()
                               .Then<TranslucentComponent>()
                               .Then<ToneMapComponent>()
                               .Show("tonemapped");

            _fullPlan.Apply();

            base.BeginTransitionOn();

            //var game = kernel.Get<TestGame>();
            _game.DisplayUI = true;
            //game.IsFixedTimeStep = true;
        }

        public override void Update(GameTime gameTime)
        {
            var keyboard = Keyboard.GetState();
            if (keyboard.IsKeyDown(Keys.D1))
                _ssaoPlan.Apply();
            else if (keyboard.IsKeyDown(Keys.D3))
                _edgeDetectPlan.Apply();
            else if (keyboard.IsKeyDown(Keys.D4))
                _lightingPlan.Apply();
            else if (keyboard.IsKeyDown(Keys.D5))
                _normalPlan.Apply();
            else if (keyboard.IsKeyDown(Keys.D6))
                _depthPlan.Apply();
            else if (keyboard.IsKeyDown(Keys.D7))
                _diffusePlan.Apply();
            else if (keyboard.IsKeyDown(Keys.D8))
                _noAaPlan.Apply();
            else
                _fullPlan.Apply();

            if (keyboard.IsKeyDown(Keys.D2))
                _ssaoIntensity.Value = 0;
            else
                _ssaoIntensity.Value = 20;

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
