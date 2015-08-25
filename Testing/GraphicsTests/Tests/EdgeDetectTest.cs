using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Myre.Graphics;
using Myre.Graphics.Deferred;
using Ninject;

using GameTime = Microsoft.Xna.Framework.GameTime;

namespace GraphicsTests.Tests
{
    class EdgeDetectTest
    : TestScreen
    {
        private readonly IKernel _kernel;
        private TestScene _scene;

        public EdgeDetectTest(
            IKernel kernel,
            ContentManager content,
            GraphicsDevice device)
            : base("Edge Detect", kernel)
        {
            _kernel = kernel;
        }

        protected override void BeginTransitionOn()
        {
            _scene = _kernel.Get<TestScene>();

            var renderer = _scene.Scene.GetService<Renderer>();
            renderer.StartPlan()
                .Then<GeometryBufferComponent>()
                .Then<EdgeDetectComponent>()
                .Show("edges")
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
