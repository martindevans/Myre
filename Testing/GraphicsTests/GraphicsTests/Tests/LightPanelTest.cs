using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Myre.Entities;
using Myre.Graphics;
using Myre.Graphics.Deferred;
using Myre.Graphics.Lighting;
using Myre.Graphics.Translucency;
using Ninject;
using Ninject.Parameters;

namespace GraphicsTests.Tests
{
    class LightPanelTest
        : TestScreen
    {
        private IKernel _kernel;
        private TestScene _scene;

        private readonly EntityDescription _planeLightDescription;
        private PlaneLight _light;

        public LightPanelTest(IKernel kernel, ContentManager content)
            :base("Light Panel", kernel)
        {
            _kernel = kernel;

            _planeLightDescription = _kernel.Get<EntityDescription>();
            _planeLightDescription.AddBehaviour<PlaneLight>();
            _planeLightDescription.AddProperty<Texture2D>(PlaneLight.TEXTURE_NAME, content.Load<Texture2D>("Kitty128"));
            _planeLightDescription.AddProperty<Vector3>(PlaneLight.POSITION_NAME, new Vector3(-100, 20, 0));
            _planeLightDescription.AddProperty<float>(PlaneLight.RANGE_NAME, 250);
            _planeLightDescription.AddProperty<Vector2>(PlaneLight.EXTENTS_NAME, new Vector2(25, 25));
            _planeLightDescription.AddProperty<Vector3>(PlaneLight.NORMAL_NAME, Vector3.Normalize(new Vector3(0, 1, 0)));
            _planeLightDescription.AddProperty<Vector3>(PlaneLight.BINORMAL_NAME, Vector3.Normalize(new Vector3(1, 0, 0)));
        }

        protected override void BeginTransitionOn()
        {
            _scene = new TestScene(_kernel, _kernel.Get<Game>(), _kernel.Get<ContentManager>(), _kernel.Get<GraphicsDevice>(), new TestScene.SceneConfiguration()
            {
                AmbientLight = false,
                RandomPointLights = 0,
                Skybox = false,
                Spotlight = false,
                Fire = false
            });

            var light = _planeLightDescription.Create();
            _light = light.GetBehaviour<PlaneLight>();
            _scene.Scene.Add(light);

            var renderer = _scene.Scene.GetService<Renderer>();
            renderer.StartPlan()
                .Then<GeometryBufferComponent>()
                .Then<EdgeDetectComponent>()
                .Then<Ssao>()
                .Then<LightingComponent>()
                .Then<ToneMapComponent>()
                .Then<TranslucentComponent>()
                .Show("lightbuffer")
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
