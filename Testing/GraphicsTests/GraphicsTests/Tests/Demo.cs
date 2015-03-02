using Microsoft.Xna.Framework;
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

namespace GraphicsTests.Tests
{
    class Demo
        : TestScreen
    {
        private IKernel kernel;
        private ContentManager content;
        private GraphicsDevice device;
        private TestScene scene;
        private TestGame game;

        private Box<float> ssaoIntensity;
        private RenderPlan fullPlan;
        private RenderPlan ssaoPlan;
        private RenderPlan lightingPlan;
        private RenderPlan edgeDetectPlan;
        private RenderPlan normalPlan;
        private RenderPlan depthPlan;
        private RenderPlan diffusePlan;

        public Demo(
            IKernel kernel,
            TestGame game,
            ContentManager content,
            GraphicsDevice device)
            : base("Demo", kernel)
        {
            this.kernel = kernel;
            this.content = content;
            this.device = device;
            this.game = game;

            //aviManager = new AviManager(@"demo.avi", false);

            //timeline = new DefaultTimeline(framesPerSecond);
            //timeline.AddAudioGroup("main");
            //videoGroup = timeline.AddVideoGroup("main", framesPerSecond, 32, width, height);
            //videoTrack = videoGroup.AddTrack();
        }

        protected override void BeginTransitionOn()
        {
            scene = kernel.Get<TestScene>();

            var particleEntityDesc = scene.Scene.Kernel.Get<EntityDescription>();
            particleEntityDesc.AddProperty(new TypedName<Vector3>("position"));
            particleEntityDesc.AddBehaviour<ParticleEmitter>();
            var entity = particleEntityDesc.Create();
            entity.GetProperty(new TypedName<Vector3>("position")).Value = Vector3.Zero;
            NamedBoxCollection initData = new NamedBoxCollection();
            initData.Set<ParticleEmitterDescription>("particlesystem", content.Load<ParticleEmitterDescription>("Particles/TestEmitter1"));
            scene.Scene.Add(entity, initData);

            var renderer = scene.Scene.GetService<Renderer>();

            ssaoIntensity = renderer.Data.Get<float>("ssao_intensity");

            fullPlan = renderer.StartPlan()
                               .Then<GeometryBufferComponent>()
                               .Then<EdgeDetectComponent>()
                               .Then<Ssao>()
                               .Then<LightingComponent>()
                               .Then<RestoreDepthPhase>()
                               .Then<TranslucentComponent>()
                               .Then<ToneMapComponent>()
                               .Then<AntiAliasComponent>()
                               .Show("antialiased");

            ssaoPlan = renderer.StartPlan()
                .Then<GeometryBufferComponent>()
                .Then<EdgeDetectComponent>()
                .Then<Ssao>()
                .Show("ssao");

            lightingPlan = renderer.StartPlan()
                .Then<GeometryBufferComponent>()
                .Then<Ssao>()
                .Then<LightingComponent>()
                .Then<RestoreDepthPhase>()
                .Then<TranslucentComponent>();
                //.Show("lightbuffer");

            edgeDetectPlan = renderer.StartPlan()
                .Then<GeometryBufferComponent>()
                .Then<EdgeDetectComponent>()
                .Show("edges");

            normalPlan = renderer.StartPlan()
                .Then<GeometryBufferComponent>()
                .Then(new AntiAliasComponent(kernel.Get<GraphicsDevice>(), "gbuffer_normals"))
                .Show("antialiased");

            depthPlan = renderer.StartPlan()
                .Then<GeometryBufferComponent>()
                .Show("gbuffer_depth");

            diffusePlan = renderer.StartPlan()
                .Then<GeometryBufferComponent>()
                .Show("gbuffer_diffuse");

            fullPlan.Apply();

            base.BeginTransitionOn();

            //var game = kernel.Get<TestGame>();
            game.DisplayUI = true;
            //game.IsFixedTimeStep = true;
        }

        public override void Update(GameTime gameTime)
        {
            var keyboard = Keyboard.GetState();
            if (keyboard.IsKeyDown(Keys.D1))
                ssaoPlan.Apply();
            else if (keyboard.IsKeyDown(Keys.D3))
                edgeDetectPlan.Apply();
            else if (keyboard.IsKeyDown(Keys.D4))
                lightingPlan.Apply();
            else if (keyboard.IsKeyDown(Keys.D5))
                normalPlan.Apply();
            else if (keyboard.IsKeyDown(Keys.D6))
                depthPlan.Apply();
            else if (keyboard.IsKeyDown(Keys.D7))
                diffusePlan.Apply();
            else
                fullPlan.Apply();

            if (keyboard.IsKeyDown(Keys.D2))
                ssaoIntensity.Value = 0;
            else
                ssaoIntensity.Value = 20;

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
