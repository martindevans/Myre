using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Myre.Entities;
using Myre.Graphics;
using Myre.Graphics.Animation;
using Myre.Graphics.Deferred;
using Myre.Graphics.Geometry;
using Myre.Graphics.Lighting;
using Myre.Graphics.Translucency;
using Ninject;

namespace GraphicsTests.Tests
{
    class AnimatedDude
        : TestScreen
    {
        private readonly Scene _scene;
        private ModelInstance _dude;

        public AnimatedDude(IKernel kernel, ContentManager content, GraphicsDevice device)
            :base("Animated Dude", kernel)
        {
            _scene = kernel.Get<Scene>();

            var model = content.Load<ModelData>(@"dude");
            var dude = kernel.Get<EntityDescription>();
            dude.AddProperty<ModelData>("model", model);
            dude.AddProperty<Matrix>("transform", Matrix.Identity);
            dude.AddProperty<bool>("is_static", true);
            dude.AddBehaviour<ModelInstance>();
            dude.AddBehaviour<Animated>();
            var dudeEntity = dude.Create();
            _scene.Add(dudeEntity);
            var animated = dudeEntity.GetBehaviour<Animated>();
            _dude = dudeEntity.GetBehaviour<ModelInstance>();
            animated.StartClip(animated.Clips.First().Value);

            var camera = new Camera();
            camera.NearClip = 1;
            camera.FarClip = 700;
            camera.View = Matrix.CreateTranslation(0, -40, 0) * Matrix.CreateLookAt(new Vector3(0, 0, -200), new Vector3(0, 0, 0), Vector3.Up);
            camera.Projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(60), 16f / 9f, camera.NearClip, camera.FarClip);
            var cameraDesc = kernel.Get<EntityDescription>();
            cameraDesc.AddProperty<Camera>("camera");
            cameraDesc.AddProperty<Viewport>("viewport");
            cameraDesc.AddBehaviour<View>();
            var cameraEntity = cameraDesc.Create();
            cameraEntity.GetProperty<Camera>("camera").Value = camera;
            cameraEntity.GetProperty<Viewport>("viewport").Value = new Viewport() { Width = device.PresentationParameters.BackBufferWidth, Height = device.PresentationParameters.BackBufferHeight };
            _scene.Add(cameraEntity);

            var ambientLight = kernel.Get<EntityDescription>();
            ambientLight.AddProperty<Vector3>("sky_colour", new Vector3(0.44f));
            ambientLight.AddProperty<Vector3>("ground_colour", new Vector3(0.24f, 0.35f, 0.24f));
            ambientLight.AddProperty<Vector3>("up", Vector3.Up);
            ambientLight.AddBehaviour<AmbientLight>();
            _scene.Add(ambientLight.Create());

            var sponza = kernel.Get<EntityDescription>();
            sponza.AddProperty<ModelData>("model", content.Load<ModelData>(@"Sponza"));
            sponza.AddProperty<Matrix>("transform", Matrix.CreateScale(0.5f) * Matrix.CreateTranslation(-350, 0, 0));
            sponza.AddProperty<bool>("is_static", true);
            sponza.AddBehaviour<ModelInstance>();
            _scene.Add(sponza.Create());

            var spotLight = kernel.Get<EntityDescription>();
            spotLight.AddProperty<Vector3>("position", new Vector3(150, 0, 0));
            spotLight.AddProperty<Vector3>("colour", new Vector3(5));
            spotLight.AddProperty<Vector3>("direction", new Vector3(-1, 0, 0));
            spotLight.AddProperty<float>("angle", MathHelper.PiOver2);
            spotLight.AddProperty<float>("range", 1000);
            spotLight.AddProperty<Texture2D>("mask", content.Load<Texture2D>("Chrysanthemum"));
            spotLight.AddProperty<int>("shadow_resolution", 1024);
            spotLight.AddBehaviour<SpotLight>();
            var spotLightEntity = spotLight.Create();
            _scene.Add(spotLightEntity);

            _scene.GetService<Renderer>().StartPlan()
                  .Then<GeometryBufferComponent>()
                  .Then<EdgeDetectComponent>()
                  .Then<Ssao>()
                  .Then<LightingComponent>()
                  .Then<ToneMapComponent>()
                  .Then<TranslucentComponent>()
                  .Apply();
        }

        public override void Update(GameTime gameTime)
        {
            _scene.Update((float) gameTime.ElapsedGameTime.TotalSeconds);
            base.Update(gameTime);

            _dude.Transform = Matrix.CreateRotationY((float) gameTime.TotalGameTime.TotalSeconds * 0.8f);
        }

        public override void Draw(GameTime gameTime)
        {
            _scene.Draw();
            base.Draw(gameTime);
        }
    }
}
