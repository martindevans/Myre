using Microsoft.Xna.Framework;
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
using Myre.Graphics.Lighting;
using Myre.Graphics.Translucency;
using Myre.Graphics.Translucency.Particles;
using Myre.UI.InputDevices;
using Ninject;
using System.Linq;
using System.Numerics;
using SwizzleMyVectors;
using SwizzleMyVectors.Geometry;
using BoundingBox = SwizzleMyVectors.Geometry.BoundingBox;
using Vector2 = System.Numerics.Vector2;
using Vector3 = System.Numerics.Vector3;
using MathHelper = Microsoft.Xna.Framework.MathHelper;
using GameTime = Microsoft.Xna.Framework.GameTime;

namespace GraphicsTests.Tests
{
    public class TransparencyTest
        : TestScreen
    {
        private readonly IKernel _kernel;
        private readonly ContentManager _content;
        private readonly GraphicsDevice _device;
        private readonly TestGame _game;
        private SpriteBatch _batch;

        private Scene _scene;

        Vector3 _cameraPosition;
        Vector3 _cameraRotation;
        private Camera _camera;

        KeyboardState _previousKeyboard;
        private View _view;

        public TransparencyTest(IKernel kernel, TestGame game, ContentManager content, GraphicsDevice device)
            : base("Transparency", kernel)
        {
            _kernel = kernel;
            _content = content;
            _device = device;
            _game = game;
            _batch = new SpriteBatch(device);
        }

        protected override void BeginTransitionOn()
        {
            _scene = _kernel.Get<Scene>();

            //Camera
            _cameraPosition = new Vector3(5, 0, -50);
            _camera = new Camera
            {
                NearClip = 1,
                FarClip = 700,
                View = Matrix4x4.CreateLookAt(_cameraPosition, new Vector3(0, 0, 0), Vector3.UnitY)
            };
            _camera.Projection = Matrix4x4.CreatePerspectiveFieldOfView(MathHelper.ToRadians(60), 16f / 9f, _camera.NearClip, _camera.FarClip);
            _cameraRotation = new Vector3(0, MathHelper.Pi, 0);

            //Camera entity
            var cameraDesc = _kernel.Get<EntityDescription>();
            cameraDesc.AddProperty(new TypedName<Camera>("camera"));
            cameraDesc.AddProperty(new TypedName<Viewport>("viewport"));
            cameraDesc.AddBehaviour<View>();
            var cameraEntity = cameraDesc.Create();
            cameraEntity.GetProperty(new TypedName<Camera>("camera")).Value = _camera;
            cameraEntity.GetProperty(new TypedName<Viewport>("viewport")).Value = new Viewport() { Width = _device.PresentationParameters.BackBufferWidth, Height = _device.PresentationParameters.BackBufferHeight };
            _scene.Add(cameraEntity);

            _view = cameraEntity.GetBehaviour<View>(null);

            //Skybox
            var skyboxDesc = _kernel.Get<EntityDescription>();
            skyboxDesc.AddBehaviour<Skybox>();
            var skybox = skyboxDesc.Create();
            skybox.GetProperty(new TypedName<TextureCube>("texture")).Value = _content.Load<TextureCube>("StormCubeMap");
            skybox.GetProperty(new TypedName<float>("brightness")).Value = 0.5f;
            skybox.GetProperty(new TypedName<bool>("gamma_correct")).Value = false;
            _scene.Add(skybox);

            //Sphere
            for (int i = 1; i < 7; i++)
            {
                var sphereModel = _content.Load<ModelData>(@"Models\sphere");
                var sphere = _kernel.Get<EntityDescription>();
                sphere.AddProperty(new TypedName<ModelData>("model"));
                sphere.AddProperty(new TypedName<Matrix4x4>("transform"));
                sphere.AddProperty(new TypedName<bool>("is_static"));
                sphere.AddBehaviour<ModelInstance>();
                var sphereEntity = sphere.Create();
                sphereEntity.GetProperty(new TypedName<ModelData>("model")).Value = sphereModel;
                sphereEntity.GetProperty(new TypedName<Matrix4x4>("transform")).Value = Matrix4x4.CreateScale(4 / sphereModel.Meshes.First().BoundingSphere.Radius)
                                                                                        * Matrix4x4.CreateRotationY(MathHelper.PiOver2)
                                                                                        * Matrix4x4.CreateTranslation(0, 0, i * 20);
                sphereEntity.GetProperty(new TypedName<bool>("is_static")).Value = true;
                _scene.Add(sphereEntity);

                var smodel = sphereEntity.GetBehaviour<ModelInstance>(null);
                smodel.Opacity = 0.15f;
                smodel.SubSurfaceScattering = 0.5f;
                smodel.Attenuation = 0.2f;
            }

            //Particles
            var particleEntityDesc = _scene.Kernel.Get<EntityDescription>();
            particleEntityDesc.AddProperty(new TypedName<Vector3>("position"));
            particleEntityDesc.AddBehaviour<ParticleEmitter>();
            var entity = particleEntityDesc.Create();
            NamedBoxCollection initData = new NamedBoxCollection();
            initData.Set(new TypedName<ParticleEmitterDescription>("particlesystem"), _content.Load<ParticleEmitterDescription>("Particles/TestEmitter1"));
            _scene.Add(entity, initData);

            _scene.GetService<Renderer>()
                  .StartPlan()
                  .Then<GeometryBufferComponent>()
                  .Then<EdgeDetectComponent>()
                  .Then<Ssao>()
                  .Then<LightingComponent>()
                  .Then<DeferredTransparency>()
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
                var resolution = renderer.Data.GetValue(Names.View.Resolution);

                var mousePosition = new Vector2(mouse.X, mouse.Y);
                var mouseDelta = mousePosition - resolution / 2;

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

                Mouse.SetPosition((int)resolution.X / 2, (int)resolution.Y / 2);
            }

            _previousKeyboard = keyboard;

            _scene.Update((float)gameTime.ElapsedGameTime.TotalSeconds);

            base.Update(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
            _scene.Draw();

            //ICollection<IGeometry> models = new List<IGeometry>();
            //_scene.FindManagers<IGeometryProvider>().ForEach(a => a.Query("translucent", _scene.GetService<Renderer>().Data, models));
            //var view = _view;
            //_batch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied);
            //foreach (var geometry in models)
            //{
            //    var bound = CalculateScreenSpaceBounds(geometry, view);
            //    _batch.Draw(_kernel.Get<ContentManager>().Load<Texture2D>("White Dot"), new Rectangle(
            //        (int)bound.Min.X,
            //        (int)bound.Min.Y,
            //        (int)(bound.Max - bound.Min).X,
            //        (int)(bound.Max - bound.Min).Y
            //    ), null, new Color(1, 1, 1, 0.5f));
            //}
            //_batch.End();

            base.Draw(gameTime);
        }

        private BoundingRectangle CalculateScreenSpaceBounds(IGeometry item, View view)
        {
            //Create a bounding box around this geometry
            var box = new BoundingBox(item.BoundingSphere);
            var corners = box.GetCorners();

            //Multiply box corners by WVP matrix to move into screen space
            for (int i = 0; i < corners.Length; i++)
            {
                corners[i] = view.Viewport.Project(corners[i].ToXNA(), view.Camera.Projection.ToXNA(), view.Camera.View.ToXNA(), Matrix.Identity).FromXNA();
            }

            //Find a rectangle around this box
            var rect = BoundingRectangle.CreateFromPoints(corners.Select(a => a.XY()));
            return rect;
        }
    }
}
