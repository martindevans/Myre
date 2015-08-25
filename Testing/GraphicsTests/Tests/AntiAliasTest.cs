using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Myre.Graphics;
using Myre.Graphics.Deferred;
using Myre.Graphics.Translucency;
using Ninject;
using GameTime = Microsoft.Xna.Framework.GameTime;

namespace GraphicsTests.Tests
{
    class AntiAliasTest
        : TestScreen
    {
        private readonly IKernel _kernel;
        private TestScene _scene;
        private Renderer _renderer;

        private RenderPlan _fullPlan;
        private RenderPlan _noAaPlan;

        public AntiAliasTest(
            IKernel kernel,
            ContentManager content,
            GraphicsDevice device)
            : base("Anti-Alias Test", kernel)
        {
            _kernel = kernel;
        }

        protected override void BeginTransitionOn()
        {
            _scene = _kernel.Get<TestScene>();

            _renderer = _scene.Scene.GetService<Renderer>();

            _fullPlan = _renderer.StartPlan()
                               .Then<GeometryBufferComponent>()
                               .Then<EdgeDetectComponent>()
                               .Then<Ssao>()
                               .Then<LightingComponent>()
                               .Then<RestoreDepthPhase>()
                               .Then<TranslucentComponent>()
                               .Then<ToneMapComponent>()
                               .Then<AntiAliasComponent>()
                               .Show("antialiased");

            _noAaPlan = _renderer.StartPlan()
                               .Then<GeometryBufferComponent>()
                               .Then<EdgeDetectComponent>()
                               .Then<Ssao>()
                               .Then<LightingComponent>()
                               .Then<RestoreDepthPhase>()
                               .Then<TranslucentComponent>()
                               .Then<ToneMapComponent>()
                               .Show("tonemapped");

            base.OnShown();
        }

        public override void Update(GameTime gameTime)
        {
            _scene.Update(gameTime);
            base.Update(gameTime);

            if (!Keyboard.GetState().IsKeyDown(Keys.F))
            {
                _fullPlan.Apply();
            }
            else
            {
                _noAaPlan.Apply();
            }
        }

        public override void Draw(GameTime gameTime)
        {
            _scene.Draw(gameTime);
            base.Draw(gameTime);
        }
    }
}
