using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Myre;
using Myre.Collections;
using Myre.Entities;
using Myre.Graphics;
using Myre.Graphics.Geometry;
using Ninject;

namespace GraphicsTests.Tests
{
    public class SpriteTest
        : TestScreen
    {
        private Scene _scene;
        private readonly IKernel _kernel;
        private readonly ContentManager _content;
        private readonly GraphicsDevice _device;

        public SpriteTest(IKernel kernel, ContentManager content, GraphicsDevice device)
            :base("Sprite Test", kernel)
        {
            _kernel = kernel;
            _content = content;
            _device = device;
        }

        protected override void BeginTransitionOn()
        {
            base.BeginTransitionOn();

            _scene = _kernel.Get<Scene>();
            _scene.GetService<Renderer>()
                  .StartPlan()
                  .Then(new CreateTargetComponent(new RenderTargetInfo(1024, 768, SurfaceFormat.Color, DepthFormat.None , 0, false, RenderTargetUsage.PreserveContents), "name"))
                  .Then<SpriteComponent>()
                  .Show("name")
                  .Apply();

            var camera = new Camera();
            camera.NearClip = 1;
            camera.FarClip = 700;
            camera.View = Matrix.CreateLookAt(new Vector3(100, 50, 0), new Vector3(0, 50, 0), Vector3.Up);
            camera.Projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(60), 16f / 9f, camera.NearClip, camera.FarClip);

            var cameraDesc = _kernel.Get<EntityDescription>();
            cameraDesc.AddProperty(new TypedName<Camera>("camera"));
            cameraDesc.AddProperty(new TypedName<Viewport>("viewport"));
            cameraDesc.AddBehaviour<View>();
            var cameraEntity = cameraDesc.Create();
            cameraEntity.GetProperty(new TypedName<Camera>("camera")).Value = camera;
            cameraEntity.GetProperty(new TypedName<Viewport>("viewport")).Value = new Viewport() {Width = _device.PresentationParameters.BackBufferWidth, Height = _device.PresentationParameters.BackBufferHeight};
            _scene.Add(cameraEntity);

            var spriteDesc = _kernel.Get<EntityDescription>();
            spriteDesc.AddBehaviour<Sprite>();

            Random r = new Random();
            for (int i = 0; i < 50; i++)
            {
                var e = spriteDesc.Create();

                _scene.Add(e, new NamedBoxCollection {
                    { Sprite.TextureName, _content.Load<Texture2D>("Chrysanthemum") },
                    { Sprite.PositionName, new Vector2(r.Next(0, 1000), r.Next(0, 1000)) },
                    { Sprite.ColorName, Color.White },
                    { Sprite.ScaleName, new Vector2(0.1f) }
                });
            }
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            _scene.Update((float) gameTime.ElapsedGameTime.TotalSeconds);
        }

        public override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);

            _scene.Draw();
        }
    }
}
