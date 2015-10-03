using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Myre;
using Myre.Entities;
using Myre.Graphics;
using Myre.Graphics.Deferred;
using Myre.Graphics.Geometry;
using Myre.Graphics.Lighting;
using Myre.Graphics.Translucency;
using Myre.UI.InputDevices;
using Ninject;
using System.Linq;
using System.Numerics;
using Vector2 = System.Numerics.Vector2;
using Vector3 = System.Numerics.Vector3;

namespace GraphicsTests.Tests
{
    public class TransparencyTest
        : TestScreen
    {
        private readonly IKernel _kernel;
        private readonly ContentManager _content;
        private readonly GraphicsDevice _device;
        private readonly TestGame _game;

        private Scene _scene;

        Vector3 _cameraPosition;
        Vector3 _cameraRotation;
        private Camera _camera;

        KeyboardState _previousKeyboard;

        public TransparencyTest(IKernel kernel, TestGame game, ContentManager content, GraphicsDevice device)
            : base("Transparency", kernel)
        {
            _kernel = kernel;
            _content = content;
            _device = device;
            _game = game;
        }

        protected override void BeginTransitionOn()
        {
            _scene = _kernel.Get<Scene>();

            //Camera
            _cameraPosition = new Vector3(100, 50, 0);
            _camera = new Camera
            {
                NearClip = 1,
                FarClip = 700,
                View = Matrix4x4.CreateLookAt(_cameraPosition, new Vector3(0, 50, 0), Vector3.UnitY)
            };
            _camera.Projection = Matrix4x4.CreatePerspectiveFieldOfView(MathHelper.ToRadians(60), 16f / 9f, _camera.NearClip, _camera.FarClip);

            //Camera entity
            var cameraDesc = _kernel.Get<EntityDescription>();
            cameraDesc.AddProperty(new TypedName<Camera>("camera"));
            cameraDesc.AddProperty(new TypedName<Viewport>("viewport"));
            cameraDesc.AddBehaviour<View>();
            var cameraEntity = cameraDesc.Create();
            cameraEntity.GetProperty(new TypedName<Camera>("camera")).Value = _camera;
            cameraEntity.GetProperty(new TypedName<Viewport>("viewport")).Value = new Viewport() { Width = _device.PresentationParameters.BackBufferWidth, Height = _device.PresentationParameters.BackBufferHeight };
            _scene.Add(cameraEntity);

            //Skybox
            var skyboxDesc = _kernel.Get<EntityDescription>();
            skyboxDesc.AddBehaviour<Skybox>();
            var skybox = skyboxDesc.Create();
            skybox.GetProperty(new TypedName<TextureCube>("texture")).Value = _content.Load<TextureCube>("StormCubeMap");
            skybox.GetProperty(new TypedName<float>("brightness")).Value = 0.5f;
            skybox.GetProperty(new TypedName<bool>("gamma_correct")).Value = false;
            _scene.Add(skybox);

            //Hebe
            var hebeModel = _content.Load<ModelData>(@"Models\Hebe2");
            var hebe = _kernel.Get<EntityDescription>();
            hebe.AddProperty(new TypedName<ModelData>("model"));
            hebe.AddProperty(new TypedName<Matrix4x4>("transform"));
            hebe.AddProperty(new TypedName<bool>("is_static"));
            hebe.AddBehaviour<ModelInstance>();
            var hebeEntity = hebe.Create();
            hebeEntity.GetProperty(new TypedName<ModelData>("model")).Value = hebeModel;
            hebeEntity.GetProperty(new TypedName<Matrix4x4>("transform")).Value = Matrix4x4.CreateScale(25 / hebeModel.Meshes.First().BoundingSphere.Radius)
                                                                    * Matrix4x4.CreateRotationY(MathHelper.PiOver2)
                                                                    * Matrix4x4.CreateTranslation(-150, 20, 0);
            hebeEntity.GetProperty(new TypedName<bool>("is_static")).Value = true;
            hebeEntity.GetProperty(ModelInstance.OpacityName).Value = 0.5f;
            _scene.Add(hebeEntity);

            _scene.GetService<Renderer>()
                  .StartPlan()
                  .Then<GeometryBufferComponent>()
                  .Then<EdgeDetectComponent>()
                  .Then<Ssao>()
                  .Then<LightingComponent>()
                  .Then<RestoreDepthPhase>()
                  .Then<TranslucentComponent>()
                  .Then<ToneMapComponent>()
                  .Then<AntiAliasComponent>()
                  .Show("antialiased")
                  .Apply();

            _game.DisplayUI = true;

            base.BeginTransitionOn();
        }

        public override void Update(GameTime gameTime)
        {
            var totalTime = (float)gameTime.TotalGameTime.TotalSeconds / 2;
            var time = (float)gameTime.ElapsedGameTime.TotalSeconds;

            MouseState mouse = Mouse.GetState();
            KeyboardState keyboard = Keyboard.GetState();

            _game.IsMouseVisible = false;
            if (mouse.IsButtonDown(MouseButtons.Right))
            {
                var renderer = _scene.GetService<Renderer>();
                var resolution = renderer.Data.Get<Vector2>("resolution");

                var mousePosition = new Vector2(mouse.X, mouse.Y);
                var mouseDelta = mousePosition - resolution.Value / 2;

                _cameraRotation.Y -= mouseDelta.X * time * 0.1f;
                _cameraRotation.X -= mouseDelta.Y * time * 0.1f;

                var rotation = Matrix4x4.CreateFromYawPitchRoll(_cameraRotation.Y, _cameraRotation.X, _cameraRotation.Z);
                var forward = Vector3.TransformNormal(-Vector3.UnitZ, rotation);
                var right = Vector3.TransformNormal(Vector3.UnitX, rotation);

                if (keyboard.IsKeyDown(Keys.W))
                    _cameraPosition += forward * time * 50;
                if (keyboard.IsKeyDown(Keys.S))
                    _cameraPosition -= forward * time * 50f;
                if (keyboard.IsKeyDown(Keys.A))
                    _cameraPosition -= right * time * 50f;
                if (keyboard.IsKeyDown(Keys.D))
                    _cameraPosition += right * time * 50f;

                Matrix4x4 invView;
                Matrix4x4.Invert(rotation * Matrix4x4.CreateTranslation(_cameraPosition), out invView);
                _camera.View = invView;

                Mouse.SetPosition((int)resolution.Value.X / 2, (int)resolution.Value.Y / 2);
            }

            _previousKeyboard = keyboard;

            _scene.Update((float)gameTime.ElapsedGameTime.TotalSeconds);

            base.Update(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
            _scene.Draw();
            base.Draw(gameTime);
        }
    }
}
