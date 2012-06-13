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
    class SsaoTest
    : TestScreen
    {
        private IKernel kernel;
        private ContentManager content;
        private GraphicsDevice device;
        private TestScene scene;

        public SsaoTest(
            IKernel kernel,
            ContentManager content,
            GraphicsDevice device)
            : base("SSAO", kernel)
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
                .Show("ssao")
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
