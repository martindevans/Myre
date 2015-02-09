using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Myre.Graphics;
using Microsoft.Xna.Framework.Graphics;
using Myre.Graphics.Translucency;
using Ninject;
using Myre.Entities;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework;
using Myre.Graphics.Geometry;
using System.IO;
using Myre.Graphics.Lighting;
using Microsoft.Xna.Framework.Input;
using Myre.UI.Gestures;
using Myre.Graphics.Deferred;

namespace GraphicsTests.Tests
{
    class AntiAliasTest
        : TestScreen
    {
        private IKernel kernel;
        private ContentManager content;
        private GraphicsDevice device;
        private TestScene scene;
        private Renderer _renderer;

        private RenderPlan _fullPlan;
        private RenderPlan _noAaPlan;

        public AntiAliasTest(
            IKernel kernel,
            ContentManager content,
            GraphicsDevice device)
            : base("Anti-Alias Test", kernel)
        {
            this.kernel = kernel;
            this.content = content;
            this.device = device;
        }

        protected override void BeginTransitionOn()
        {
            scene = kernel.Get<TestScene>();

            _renderer = scene.Scene.GetService<Renderer>();

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
            scene.Update(gameTime);
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
            scene.Draw(gameTime);
            base.Draw(gameTime);
        }
    }
}
