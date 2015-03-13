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
using Myre.Graphics.Deferred.Decals;
using Myre.Graphics.Geometry;
using Myre.Graphics.Lighting;
using Myre.Graphics.Translucency;
using Myre.UI.InputDevices;
using Ninject;

namespace GraphicsTests.Tests
{
    class DecalTest
        : TestScreen
    {
        private readonly Scene _scene;

        Vector3 _cameraPosition;
        Vector3 _cameraRotation;
        private readonly Camera _camera;

        public DecalTest(IKernel kernel, ContentManager content, GraphicsDevice device)
            : base("Decal Test", kernel)
        {
            _scene = kernel.Get<Scene>();

            _scene.GetService<Renderer>().StartPlan()
                  .Then<GeometryBufferComponent>()
                  .Then<DecalComponent>()
                  .Then<DecalMixComponent>()
                  .Then<Ssao>()
                  .Then<LightingComponent>()
                  .Then<TranslucentComponent>()
                  .Then<ToneMapComponent>()
                  .Then<AntiAliasComponent>()
                  //.Show("gbuffer_normals")
                  .Apply();

            _camera = new Camera { NearClip = 1, FarClip = 7000, View = Matrix.CreateLookAt(new Vector3(100, 50, -200), new Vector3(0, 0, 0), Vector3.Up) };
            _camera.Projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(60), 16f / 9f, _camera.NearClip, _camera.FarClip);
            var cameraDesc = kernel.Get<EntityDescription>();
            cameraDesc.AddProperty(new TypedName<Camera>("camera"));
            cameraDesc.AddProperty(new TypedName<Viewport>("viewport"));
            cameraDesc.AddBehaviour<View>();
            var cameraEntity = cameraDesc.Create();
            cameraEntity.GetProperty(new TypedName<Camera>("camera")).Value = _camera;
            cameraEntity.GetProperty(new TypedName<Viewport>("viewport")).Value = new Viewport() { Width = device.PresentationParameters.BackBufferWidth, Height = device.PresentationParameters.BackBufferHeight };
            _scene.Add(cameraEntity);

            var ambientLight = kernel.Get<EntityDescription>();
            ambientLight.AddProperty(new TypedName<Vector3>("sky_colour"), new Vector3(0.44f, 0.44f, 0.74f));
            ambientLight.AddProperty(new TypedName<Vector3>("ground_colour"), new Vector3(0.24f, 0.35f, 0.24f));
            ambientLight.AddProperty(new TypedName<Vector3>("up"), Vector3.Up);
            ambientLight.AddBehaviour<AmbientLight>();
            _scene.Add(ambientLight.Create());

            var sponza = kernel.Get<EntityDescription>();
            sponza.AddProperty(new TypedName<ModelData>("model"), content.Load<ModelData>(@"Sponza"));
            sponza.AddProperty(new TypedName<Matrix>("transform"), Matrix.CreateScale(0.5f) * Matrix.CreateTranslation(-350, 0, 0));
            sponza.AddProperty(new TypedName<bool>("is_static"), true);
            sponza.AddBehaviour<ModelInstance>();
            _scene.Add(sponza.Create());

            var spotLight = kernel.Get<EntityDescription>();
            spotLight.AddProperty(new TypedName<Vector3>("position"), new Vector3(150, 50, -50));
            spotLight.AddProperty(new TypedName<Vector3>("colour"), new Vector3(1));
            spotLight.AddProperty(new TypedName<Vector3>("direction"), new Vector3(-1, 0, 0.25f));
            spotLight.AddProperty(new TypedName<float>("angle"), MathHelper.PiOver2);
            spotLight.AddProperty(new TypedName<float>("range"), 1000);
            spotLight.AddProperty(new TypedName<int>("shadow_resolution"), 1024);
            spotLight.AddBehaviour<SpotLight>();
            var spotLightEntity = spotLight.Create();
            _scene.Add(spotLightEntity);

            var decal = kernel.Get<EntityDescription>();
            decal.AddBehaviour<Decal>();

            //Random r = new Random(2);
            //for (int i = 0; i < 150; i++)
            //{
            //    _scene.Add(decal.Create(), new NamedBoxCollection {
            //        { Decal.NormalName, content.Load<Texture2D>("randomnormals") },
            //        { Decal.DiffuseName, content.Load<Texture2D>("Splatter") },
            //        { Decal.TransformName, Matrix.CreateScale(30, 5, 30) * Matrix.CreateRotationX(MathHelper.PiOver2) * Matrix.CreateRotationY(MathHelper.PiOver2) * Matrix.CreateTranslation(-130, r.Next(-100, 100), r.Next(-100, 100)) },
            //        { Decal.AngleCutoffName, MathHelper.PiOver4 }
            //    });
            //}

            _scene.Add(decal.Create(), new NamedBoxCollection {
                { Decal.NormalName, content.Load<Texture2D>("randomnormals") },
                { Decal.DiffuseName, content.Load<Texture2D>("Splatter") },
                { Decal.TransformName, Matrix.CreateScale(30, 30, 30) * Matrix.CreateRotationX(MathHelper.PiOver2 + MathHelper.PiOver4) * Matrix.CreateRotationY(MathHelper.PiOver2) * Matrix.CreateTranslation(-135, -10, 0) },
                { Decal.AngleCutoffName, MathHelper.Pi / 3.65f }
            });

            _scene.Add(decal.Create(), new NamedBoxCollection {
                { Decal.NormalName, content.Load<Texture2D>("randomnormals") },
                { Decal.DiffuseName, content.Load<Texture2D>("Splatter") },
                { Decal.TransformName, Matrix.CreateScale(30, 30, 30) * Matrix.CreateRotationX(MathHelper.PiOver2 + MathHelper.PiOver4) * Matrix.CreateRotationY(MathHelper.PiOver2) * Matrix.CreateTranslation(-135, -10, 40) },
                { Decal.AngleCutoffName, MathHelper.Pi / 3 }
            });

            _scene.Add(decal.Create(), new NamedBoxCollection {
                { Decal.NormalName, content.Load<Texture2D>("randomnormals") },
                { Decal.DiffuseName, content.Load<Texture2D>("Splatter") },
                { Decal.TransformName, Matrix.CreateScale(30, 30, 30) * Matrix.CreateRotationX(MathHelper.PiOver2 + MathHelper.PiOver4) * Matrix.CreateRotationY(MathHelper.PiOver2) * Matrix.CreateTranslation(-135, -10, 80) },
                { Decal.AngleCutoffName, MathHelper.Pi / 2 }
            });

            _scene.Add(decal.Create(), new NamedBoxCollection {
                { Decal.NormalName, content.Load<Texture2D>("randomnormals") },
                { Decal.DiffuseName, content.Load<Texture2D>("Splatter") },
                { Decal.TransformName, Matrix.CreateScale(30, 30, 30) * Matrix.CreateRotationX(MathHelper.PiOver2 + MathHelper.PiOver4) * Matrix.CreateRotationY(MathHelper.PiOver2) * Matrix.CreateTranslation(-135, -10, 120) },
                { Decal.AngleCutoffName, MathHelper.Pi }
            });
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            _scene.Update((float)gameTime.ElapsedGameTime.TotalSeconds);

            var deltaTime = gameTime.Seconds();
            var time = gameTime.TotalSeconds();

            MouseState mouse = Mouse.GetState();
            KeyboardState keyboard = Keyboard.GetState();

            Game.IsMouseVisible = false;
            if (mouse.IsButtonDown(MouseButtons.Right))
            {
                var resolution = new Vector2(Game.GraphicsDevice.Viewport.Width, Game.GraphicsDevice.Viewport.Height);
                var mousePosition = new Vector2(mouse.X, mouse.Y);
                var mouseDelta = mousePosition - resolution / 2;

                _cameraRotation.Y -= mouseDelta.X * deltaTime * 0.1f;
                _cameraRotation.X -= mouseDelta.Y * deltaTime * 0.1f;

                var rotation = Matrix.CreateFromYawPitchRoll(_cameraRotation.Y, _cameraRotation.X, _cameraRotation.Z);
                var forward = Vector3.TransformNormal(Vector3.Forward, rotation);
                var right = Vector3.TransformNormal(Vector3.Right, rotation);

                forward.Normalize();
                right.Normalize();

                if (keyboard.IsKeyDown(Keys.W))
                    _cameraPosition += forward * deltaTime * 50;
                if (keyboard.IsKeyDown(Keys.S))
                    _cameraPosition -= forward * deltaTime * 50f;
                if (keyboard.IsKeyDown(Keys.A))
                    _cameraPosition -= right * deltaTime * 50f;
                if (keyboard.IsKeyDown(Keys.D))
                    _cameraPosition += right * deltaTime * 50f;

                _camera.View = Matrix.CreateLookAt(_cameraPosition, _cameraPosition + forward, Vector3.Cross(right, forward));

                Mouse.SetPosition((int)resolution.X / 2, (int)resolution.Y / 2);
            }
        }

        public override void Draw(GameTime gameTime)
        {
            _scene.Draw();
            base.Draw(gameTime);
        }
    }
}
