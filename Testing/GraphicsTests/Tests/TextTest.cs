using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Myre;
using Myre.Collections;
using Myre.Entities;
using Myre.Extensions;
using Myre.Graphics;
using Myre.Graphics.Deferred;
using Myre.Graphics.Geometry;
using Myre.Graphics.Geometry.Text;
using Myre.Graphics.Lighting;
using Myre.UI.InputDevices;
using Ninject;
using System.Numerics;

using GameTime = Microsoft.Xna.Framework.GameTime;
using MathHelper = Microsoft.Xna.Framework.MathHelper;

namespace GraphicsTests.Tests
{
    class TextTest
        : TestScreen
    {
        private readonly IKernel _kernel;
        private readonly GraphicsDevice _device;
        private readonly ContentManager _content;

        private Scene _scene;
        private Camera _camera;

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
            _resolution = renderer.Data.Get<Vector2>("resolution");

            //Create camera
            _camera = new Camera { NearClip = 1, FarClip = 7000, View = Matrix4x4.CreateLookAt(new Vector3(-100, 300, 10), new Vector3(300, 0, 0), -Vector3.UnitZ) };
            _camera.Projection = Matrix4x4.CreatePerspectiveFieldOfView(MathHelper.ToRadians(60), 16f / 9f, _camera.NearClip, _camera.FarClip);
            var cameraDesc = _kernel.Get<EntityDescription>();
            cameraDesc.AddProperty(new TypedName<Camera>("camera"));
            cameraDesc.AddProperty(new TypedName<Viewport>("viewport"));
            cameraDesc.AddBehaviour<View>();
            var cameraEntity = cameraDesc.Create();
            cameraEntity.GetProperty(new TypedName<Camera>("camera")).Value = _camera;
            cameraEntity.GetProperty(new TypedName<Viewport>("viewport")).Value = new Viewport() { Width = _device.PresentationParameters.BackBufferWidth, Height = _device.PresentationParameters.BackBufferHeight };
            _scene.Add(cameraEntity);

            //create ambient light
            var ambientLight = _kernel.Get<EntityDescription>();
            ambientLight.AddProperty(new TypedName<Vector3>("sky_colour"));
            ambientLight.AddProperty(new TypedName<Vector3>("ground_colour"));
            ambientLight.AddProperty(new TypedName<Vector3>("up"));
            ambientLight.AddBehaviour<AmbientLight>();
            var ambientLightEntity = ambientLight.Create();
            ambientLightEntity.GetProperty(new TypedName<Vector3>("sky_colour")).Value = new Vector3(0.08f);
            ambientLightEntity.GetProperty(new TypedName<Vector3>("ground_colour")).Value = new Vector3(0.04f, 0.05f, 0.04f);
            ambientLightEntity.GetProperty(new TypedName<Vector3>("up")).Value = Vector3.UnitY;
            _scene.Add(ambientLightEntity);

            //Create text
            var textDesc = _kernel.Get<EntityDescription>();
            textDesc.AddBehaviour<ModelInstance>();
            textDesc.AddProperty(ModelInstance.TransformName, Matrix4x4.Identity);
            textDesc.AddBehaviour<StringModelData>();
            var textEnt = textDesc.Create();
            var init = new NamedBoxCollection {
                { StringModelData.FontName, _content.Load<VertexFont>("Fonts/Cousine-Regular-Latin") },
                { StringModelData.StringName, "Hello, World" },
                { StringModelData.ThicknessName, 25 },
            };

            _scene.Add(textEnt, init);
        }

        private Vector3 _cameraRotation;
        private Vector3 _cameraPosition;
        private Box<Vector2> _resolution;

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            _scene.Update((float)gameTime.ElapsedGameTime.TotalSeconds);

            MouseState mouse = Mouse.GetState();
            KeyboardState keyboard = Keyboard.GetState();

            Game.IsMouseVisible = false;
            if (mouse.IsButtonDown(MouseButtons.Right))
            {
                var mousePosition = new Vector2(mouse.X, mouse.Y);
                var mouseDelta = mousePosition - _resolution.Value / 2;

                _cameraRotation.Y -= mouseDelta.X * gameTime.Seconds() * 0.1f;
                _cameraRotation.X -= mouseDelta.Y * gameTime.Seconds() * 0.1f;

                var rotation = Matrix4x4.CreateFromYawPitchRoll(_cameraRotation.Y, _cameraRotation.X, _cameraRotation.Z);
                var forward = Vector3.TransformNormal(-Vector3.UnitZ, rotation);
                var right = Vector3.TransformNormal(Vector3.UnitX, rotation);

                if (keyboard.IsKeyDown(Keys.W))
                    _cameraPosition += forward * gameTime.Seconds() * 50;
                if (keyboard.IsKeyDown(Keys.S))
                    _cameraPosition -= forward * gameTime.Seconds() * 50f;
                if (keyboard.IsKeyDown(Keys.A))
                    _cameraPosition -= right * gameTime.Seconds() * 50f;
                if (keyboard.IsKeyDown(Keys.D))
                    _cameraPosition += right * gameTime.Seconds() * 50f;

                Matrix4x4 invView;
                Matrix4x4.Invert(rotation * Matrix4x4.CreateTranslation(_cameraPosition), out invView);
                _camera.View = invView;

                Mouse.SetPosition((int)_resolution.Value.X / 2, (int)_resolution.Value.Y / 2);
                //camera.View = Matrix.CreateLookAt(new Vector3(0, 60, -7), new Vector3(50, 30, -50), Vector3.Up);
            }
        }

        public override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);

            _scene.Draw();
        }
    }
}
