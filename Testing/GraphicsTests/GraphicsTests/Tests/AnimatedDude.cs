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
            ambientLight.AddProperty<Vector3>("sky_colour");
            ambientLight.AddProperty<Vector3>("ground_colour");
            ambientLight.AddProperty<Vector3>("up");
            ambientLight.AddBehaviour<AmbientLight>();
            var ambientLightEntity = ambientLight.Create();
            ambientLightEntity.GetProperty<Vector3>("sky_colour").Value = new Vector3(0.24f);
            ambientLightEntity.GetProperty<Vector3>("ground_colour").Value = new Vector3(0.14f, 0.15f, 0.14f);
            ambientLightEntity.GetProperty<Vector3>("up").Value = Vector3.Up;
            _scene.Add(ambientLightEntity);

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
        }

        public override void Draw(GameTime gameTime)
        {
            _scene.Draw();
            base.Draw(gameTime);
        }
    }
}
