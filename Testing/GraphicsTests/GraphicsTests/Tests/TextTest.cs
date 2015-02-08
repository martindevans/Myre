using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Myre;
using Myre.Collections;
using Myre.Entities;
using Myre.Graphics;
using Myre.Graphics.Deferred;
using Myre.Graphics.Geometry;
using Myre.Graphics.Geometry.Text;
using Ninject;

namespace GraphicsTests.Tests
{
    class TextTest
        : TestScreen
    {
        private readonly IKernel _kernel;
        private readonly GraphicsDevice _device;
        private readonly ContentManager _content;

        private Scene _scene;

        public TextTest(IKernel kernel, GraphicsDevice device, ContentManager content)
            : base("Text Test", kernel)
        {
            _kernel = kernel;
            _device = device;
            _content = content;
        }

        protected override void BeginTransitionOn()
        {
            base.BeginTransitionOn();

            _scene = _kernel.Get<Scene>();

            //Start renderer
            var renderer = _scene.GetService<Renderer>();
            renderer.StartPlan()
                .Then<GeometryBufferComponent>()
                .Then<EdgeDetectComponent>()
                .Then<Ssao>()
                .Then<LightingComponent>()
                .Then<RestoreDepthPhase>()
                .Then<ToneMapComponent>()
                .Then<AntiAliasComponent>()
                .Show("antialiased")
                .Apply();

            //Create camera
            var camera = new Camera { NearClip = 1, FarClip = 7000, View = Matrix.CreateLookAt(new Vector3(100, 0, 100), new Vector3(0, 0, 0), Vector3.Up) };
            camera.Projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(60), 16f / 9f, camera.NearClip, camera.FarClip);
            var cameraDesc = _kernel.Get<EntityDescription>();
            cameraDesc.AddProperty(new TypedName<Camera>("camera"));
            cameraDesc.AddProperty(new TypedName<Viewport>("viewport"));
            cameraDesc.AddBehaviour<View>();
            var cameraEntity = cameraDesc.Create();
            cameraEntity.GetProperty(new TypedName<Camera>("camera")).Value = camera;
            cameraEntity.GetProperty(new TypedName<Viewport>("viewport")).Value = new Viewport() { Width = _device.PresentationParameters.BackBufferWidth, Height = _device.PresentationParameters.BackBufferHeight };
            _scene.Add(cameraEntity);

            //Create text
            var textDesc = _kernel.Get<EntityDescription>();
            textDesc.AddBehaviour<ModelInstance>();
            textDesc.AddBehaviour<StringModelData>();
            var textEnt = textDesc.Create();
            var init = new NamedBoxCollection {
                { StringModelData.FontName, _content.Load<VertexFont>("Fonts/Cousine-Regular-Latin") },
                { StringModelData.StringName, "Hello, World" },
                { StringModelData.ThicknessName, 1 },
            };

            _scene.Add(textEnt, init);
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            _scene.Update((float)gameTime.ElapsedGameTime.TotalSeconds);
        }

        public override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);

            _scene.Draw();
        }
    }
}
