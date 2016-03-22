using System.Numerics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Myre;
using Myre.Collections;
using Myre.Debugging.UI;
using Myre.Entities;
using Myre.Graphics;
using Myre.Graphics.Geometry;
using Myre.Graphics.Lighting;
using Myre.UI.InputDevices;
using Ninject;
using System;
using System.Collections.Generic;
using System.Linq;

using Game = Microsoft.Xna.Framework.Game;
using GameTime = Microsoft.Xna.Framework.GameTime;
using MathHelper = Microsoft.Xna.Framework.MathHelper;
using Vector2 = System.Numerics.Vector2;
using Vector3 = System.Numerics.Vector3;

namespace GraphicsTests
{
    class TestScene
    {
        readonly Game _game;
        readonly Scene _scene;
        readonly Camera _camera;
        readonly List<PointLight> _lights;
        readonly SpotLight _spotLight;
        readonly Box<Vector2> _resolution;

        bool _paused;
        KeyboardState _previousKeyboard;

        Vector3 _cameraPosition;
        Vector3 _cameraRotation;
        readonly CameraScript _cameraScript;

        public Scene Scene
        {
            get { return _scene; }
        }

        public Camera Camera
        {
            get { return _camera; }
        }

        public class SceneConfiguration
        {
            public bool Skybox = true;
            public int RandomPointLights = 0;
            public bool Spotlight = true;
            public bool AmbientLight = true;
            public bool Fire = true;
            public bool SunLight = false;
            public bool View = true;
        }

        public TestScene(IKernel kernel, Game game, ContentManager content, GraphicsDevice device, [Optional]SceneConfiguration config)
        {
            config = config ?? new SceneConfiguration();

            _scene = new Scene(kernel);
            _game = game;

            _cameraPosition = new Vector3(100, 50, 0);
            
            _camera = new Camera();
            _camera.NearClip = 1;
            _camera.FarClip = 700;
            _camera.View = Matrix4x4.CreateLookAt(_cameraPosition, new Vector3(0, 50, 0), Vector3.UnitY);
            _camera.Projection = Matrix4x4.CreatePerspectiveFieldOfView(MathHelper.ToRadians(60), 16f / 9f, _camera.NearClip, _camera.FarClip);

            if (config.View)
            {
                var cameraDesc = kernel.Get<EntityDescription>();
                cameraDesc.AddProperty(new TypedName<Camera>("camera"));
                cameraDesc.AddProperty(new TypedName<Viewport>("viewport"));
                cameraDesc.AddBehaviour<View>();
                var cameraEntity = cameraDesc.Create();
                cameraEntity.GetProperty(new TypedName<Camera>("camera")).Value = _camera;
                cameraEntity.GetProperty(new TypedName<Viewport>("viewport")).Value = new Viewport() {Width = device.PresentationParameters.BackBufferWidth, Height = device.PresentationParameters.BackBufferHeight};
                _scene.Add(cameraEntity);
            }

            if (config.Skybox)
            {
                var skyboxDesc = kernel.Get<EntityDescription>();
                skyboxDesc.AddBehaviour<Skybox>();
                var skybox = skyboxDesc.Create();
                skybox.GetProperty(new TypedName<TextureCube>("texture")).Value = content.Load<TextureCube>("StormCubeMap");
                skybox.GetProperty(new TypedName<float>("brightness")).Value = 0.5f;
                skybox.GetProperty(new TypedName<bool>("gamma_correct")).Value = false;
                _scene.Add(skybox);
            }

            var pointLight = kernel.Get<EntityDescription>();
            pointLight.AddProperty(new TypedName<Vector3>("position"));
            pointLight.AddProperty(new TypedName<Vector3>("colour"));
            pointLight.AddProperty(new TypedName<float>("range"));
            pointLight.AddBehaviour<PointLight>();
            //scene.Add(pointLight.Create());

            _lights = new List<PointLight>();
            var rng = new Random();
            for (int i = 0; i < config.RandomPointLights; i++)
            {
                var entity = pointLight.Create();
                _scene.Add(entity);

                entity.GetProperty(new TypedName<Vector3>("position")).Value = new Vector3(0, 10, 0);
                entity.GetProperty(new TypedName<Vector3>("colour")).Value = new Vector3(0, 5, 0);
                entity.GetProperty(new TypedName<float>("range")).Value = 200;

                var light = entity.GetBehaviour<PointLight>(null);
                light.Colour = Vector3.Normalize(new Vector3(0.1f + (float)rng.NextDouble(), 0.1f + (float)rng.NextDouble(), 0.1f + (float)rng.NextDouble())) * 10;
                _lights.Add(light);
            }

            if (config.Spotlight)
            {
                var spotLight = kernel.Get<EntityDescription>();
                spotLight.AddProperty(new TypedName<Vector3>("position"));
                spotLight.AddProperty(new TypedName<Vector3>("colour"));
                spotLight.AddProperty(new TypedName<Vector3>("direction"));
                spotLight.AddProperty(new TypedName<float>("angle"));
                spotLight.AddProperty(new TypedName<float>("range"));
                spotLight.AddProperty(new TypedName<Texture2D>("mask"));
                spotLight.AddProperty(new TypedName<int>("shadow_resolution"));
                spotLight.AddBehaviour<SpotLight>();
                var spotLightEntity = spotLight.Create();
                spotLightEntity.GetProperty(new TypedName<Vector3>("position")).Value = new Vector3(-180, 250, 0);
                spotLightEntity.GetProperty(new TypedName<Vector3>("colour")).Value = new Vector3(10);
                spotLightEntity.GetProperty(new TypedName<Vector3>("direction")).Value = new Vector3(0, -1, 0);
                spotLightEntity.GetProperty(new TypedName<float>("angle")).Value = MathHelper.PiOver2;
                spotLightEntity.GetProperty(new TypedName<float>("range")).Value = 500;
                spotLightEntity.GetProperty(new TypedName<Texture2D>("mask")).Value = content.Load<Texture2D>("Chrysanthemum");
                spotLightEntity.GetProperty(new TypedName<int>("shadow_resolution")).Value = 1024;
                _spotLight = spotLightEntity.GetBehaviour<SpotLight>(null);
                _scene.Add(spotLightEntity);
            }

            if (config.AmbientLight)
            {
                var ambientLight = kernel.Get<EntityDescription>();
                ambientLight.AddProperty(new TypedName<Vector3>("sky_colour"));
                ambientLight.AddProperty(new TypedName<Vector3>("ground_colour"));
                ambientLight.AddProperty(new TypedName<Vector3>("up"));
                ambientLight.AddBehaviour<AmbientLight>();
                var ambientLightEntity = ambientLight.Create();
                ambientLightEntity.GetProperty(new TypedName<Vector3>("sky_colour")).Value = new Vector3(0.04f);
                ambientLightEntity.GetProperty(new TypedName<Vector3>("ground_colour")).Value = new Vector3(0.04f, 0.05f, 0.04f);
                ambientLightEntity.GetProperty(new TypedName<Vector3>("up")).Value = Vector3.UnitY;
                _scene.Add(ambientLightEntity);
            }

            if (config.SunLight)
            {
                var sunlight = kernel.Get<EntityDescription>();
                sunlight.AddBehaviour<SunLight>();
                var sunlightEntity = sunlight.Create();
                sunlightEntity.GetProperty(new TypedName<Vector3>("colour")).Value = new Vector3(1, 0.75f, 0.6f);
                sunlightEntity.GetProperty(new TypedName<Vector3>("direction")).Value = -Vector3.UnitY;
                sunlightEntity.GetProperty(new TypedName<int>("shadow_resolution")).Value = 1024;
                _scene.Add(sunlightEntity);

                //var sunEntity = kernel.Get<EntityDescription>();
                //sunEntity.AddProperty(SunLight.DirectionName, Vector3.Normalize(new Vector3(-.2f, -1f, .3f)));
                //sunEntity.AddProperty(SunLight.ColourName, new Vector3(1, 0.3f, 0.01f) * 5);
                //sunEntity.AddProperty(SunLight.ShadowResolutionName, 4096);
                //sunEntity.AddProperty(SunLight.ActiveName, true);
                //sunEntity.AddBehaviour<SunLight>();
                //Entity sun = sunEntity.Create();
                //_scene.Add(sun);

                //var sun2 = sunEntity.Create();
                //sun2.GetProperty<Vector3>("direction").Value = Vector3.Normalize(new Vector3(1, -1, 0));
                //sun2.GetProperty<Vector3>("colour").Value = new Vector3(1, 0, 0);
                //scene.Add(sun2);
            }

            //var floor = content.Load<ModelData>(@"Models\Ground");
            //var floorEntity = kernel.Get<EntityDescription>();
            //floorEntity.AddProperty<ModelData>("model", floor);
            //floorEntity.AddProperty<Matrix>("transform", Matrix.CreateScale(2));
            //floorEntity.AddProperty<bool>("isstatic", true);
            //floorEntity.AddBehaviour<ModelInstance>();
            //scene.Add(floorEntity.Create());

            //var ship1 = content.Load<ModelData>(@"Models\Ship1");
            //var ship1Entity = kernel.Get<EntityDescription>();
            //ship1Entity.AddProperty<ModelData>("model", ship1);
            //ship1Entity.AddProperty<Matrix>("transform", Matrix.CreateTranslation(30, 0, 0));
            //ship1Entity.AddProperty<bool>("is_static", true);
            //ship1Entity.AddBehaviour<ModelInstance>();
            //scene.Add(ship1Entity.Create());

            //var hebeModel = content.Load<ModelData>(@"Models\Hebe2");
            //var hebe = kernel.Get<EntityDescription>();
            //hebe.AddProperty(new TypedName<ModelData>("model"));
            //hebe.AddProperty(new TypedName<Matrix4x4>("transform"));
            //hebe.AddProperty(new TypedName<bool>("is_static"));
            //hebe.AddBehaviour<ModelInstance>();
            //var hebeEntity = hebe.Create();
            //hebeEntity.GetProperty(new TypedName<ModelData>("model")).Value = hebeModel;
            //hebeEntity.GetProperty(new TypedName<Matrix4x4>("transform")).Value = Matrix4x4.CreateScale(25 / hebeModel.Meshes.First().BoundingSphere.Radius)
            //                                                        * Matrix4x4.CreateRotationY(MathHelper.PiOver2)
            //                                                        * Matrix4x4.CreateTranslation(-150, 20, 0);
            //hebeEntity.GetProperty(new TypedName<bool>("is_static")).Value = true;
            //hebeEntity.GetProperty(ModelInstance.OpacityName).Value = 0.5f;
            //_scene.Add(hebeEntity);

            var sphereModel = content.Load<ModelData>(@"Models\sphere");
            var sphere = kernel.Get<EntityDescription>();
            sphere.AddProperty(new TypedName<ModelData>("model"));
            sphere.AddProperty(new TypedName<Matrix4x4>("transform"));
            sphere.AddProperty(new TypedName<bool>("is_static"));
            sphere.AddBehaviour<ModelInstance>();
            var sphereEntity = sphere.Create();
            sphereEntity.GetProperty(new TypedName<ModelData>("model")).Value = sphereModel;
            sphereEntity.GetProperty(new TypedName<Matrix4x4>("transform")).Value = Matrix4x4.CreateScale(5 / sphereModel.Meshes.First().BoundingSphere.Radius)
                                                                    * Matrix4x4.CreateRotationY(MathHelper.PiOver2)
                                                                    * Matrix4x4.CreateTranslation(-150, 20, 0);
            sphereEntity.GetProperty(new TypedName<bool>("is_static")).Value = true;
            _scene.Add(sphereEntity);

            var smodel = sphereEntity.GetBehaviour<ModelInstance>(null);
            smodel.Opacity = 0.5f;
            smodel.SubSurfaceScattering = 0.5f;
            smodel.Attenuation = 0.3f;

            //var dudeModel = content.Load<ModelData>(@"dude");
            //var dude = kernel.Get<EntityDescription>();
            //dude.AddProperty<ModelData>("model", dudeModel);
            //dude.AddProperty<Matrix>("transform", Matrix.CreateScale(0.75f) * Matrix.CreateTranslation(-50, 0, 0));
            //dude.AddProperty<bool>("is_static", true);
            //dude.AddBehaviour<ModelInstance>();
            //dude.AddBehaviour<Animated>();
            //var dudeEntity = dude.Create();
            //scene.Add(dudeEntity);

            var sponzaModel = content.Load<ModelData>(@"Sponza");
            var sponza = kernel.Get<EntityDescription>();
            sponza.AddProperty(new TypedName<ModelData>("model"));
            sponza.AddProperty(new TypedName<Matrix4x4>("transform"));
            sponza.AddProperty(new TypedName<bool>("is_static"));
            sponza.AddBehaviour<ModelInstance>();
            var sponzaEntity = sponza.Create();
            sponzaEntity.GetProperty(new TypedName<ModelData>("model")).Value = sponzaModel;
            sponzaEntity.GetProperty(new TypedName<Matrix4x4>("transform")).Value = Matrix4x4.Identity * Matrix4x4.CreateScale(1);
            sponzaEntity.GetProperty(new TypedName<bool>("is_static")).Value = true;
            _scene.Add(sponzaEntity);

            var renderer = _scene.GetService<Renderer>();
            _resolution = renderer.Data.GetOrCreate(Names.View.Resolution);

            var console = kernel.Get<CommandConsole>();
            renderer.Settings.BindCommandEngine(console.Engine);

            if (config.Fire)
            {
                //var fire1 = Fire.Create(kernel, content, new Vector3(123.5f, 30f, -55f));
                //var fire2 = Fire.Create(kernel, content, new Vector3(123.5f, 30f, 35f));
                //var fire3 = Fire.Create(kernel, content, new Vector3(-157f, 30f, 35f));
                //var fire4 = Fire.Create(kernel, content, new Vector3(-157f, 30f, -55f));

                //scene.Add(fire1);
                //scene.Add(fire2);
                //scene.Add(fire3);
                //scene.Add(fire4);
            }

            _cameraScript = new CameraScript(_camera);
            _cameraScript.AddWaypoint(0, new Vector3(218, 160, 104), new Vector3(0, 150, 0));
            _cameraScript.AddWaypoint(10, new Vector3(-195, 160, 104), new Vector3(-150, 150, 0));
            _cameraScript.AddWaypoint(12, new Vector3(-270, 160, 96), new Vector3(-150, 150, 0));
            _cameraScript.AddWaypoint(14, new Vector3(-302, 160, 45), new Vector3(-150, 150, 0));
            _cameraScript.AddWaypoint(16, new Vector3(-286, 160, 22), new Vector3(-150, 150, 0));
            _cameraScript.AddWaypoint(18, new Vector3(-276, 160, 22), new Vector3(-150, 100, 0));
            _cameraScript.AddWaypoint(20, new Vector3(-158, 42, 19), new Vector3(-150, 40, 0));
            _cameraScript.AddWaypoint(21, new Vector3(-105, 24, -7), new Vector3(-150, 40, 0));
            _cameraScript.AddWaypoint(23, new Vector3(-105, 44, -7), new Vector3(-150, 40, 0));
            _cameraScript.AddWaypoint(27, new Vector3(-105, 50, -7), new Vector3(-80, 50, -100));
            _cameraScript.AddWaypoint(32, new Vector3(100, 50, -7), new Vector3(150, 40, 0));
            _cameraScript.AddWaypoint(34, new Vector3(100, 50, -7), new Vector3(150, 40, 100));
            _cameraScript.AddWaypoint(36, new Vector3(100, 50, -7), new Vector3(0, 60, 0));
            //cameraScript.AddWaypoint(1000, new Vector3(100, 50, -7), new Vector3(0, 60, 0));
            _cameraScript.Initialise();
        }

        public void Update(GameTime gameTime)
        {
            var totalTime = (float)gameTime.TotalGameTime.TotalSeconds / 2;
            var time = (float)gameTime.ElapsedGameTime.TotalSeconds;

            MouseState mouse = Mouse.GetState();
            KeyboardState keyboard = Keyboard.GetState();

            var spotlightTransform = Matrix4x4.CreateRotationX((float)Math.Sin(totalTime));
            _spotLight.Direction = Vector3.TransformNormal(-Vector3.UnitY, spotlightTransform);
            _spotLight.Up = Vector3.TransformNormal(Vector3.UnitZ, spotlightTransform);

            _game.IsMouseVisible = false;
            if (mouse.IsButtonDown(MouseButtons.Right))
            {
                var mousePosition = new Vector2(mouse.X, mouse.Y);
                var mouseDelta = mousePosition - _resolution.Value / 2;

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

                Mouse.SetPosition((int)_resolution.Value.X / 2, (int)_resolution.Value.Y / 2);
                //camera.View = Matrix.CreateLookAt(new Vector3(0, 60, -7), new Vector3(50, 30, -50), Vector3.Up);
            }
            else
            {
                if (keyboard.IsKeyDown(Keys.Space) && _previousKeyboard.IsKeyUp(Keys.Space))
                    _paused = !_paused;

                if (!_paused)
                    _cameraScript.Update((float)gameTime.ElapsedGameTime.TotalSeconds);

                _cameraPosition = _cameraScript.Position;
            }

            _previousKeyboard = keyboard;

            //camera.View = spotLight.view;
            //camera.Projection = spotLight.projection;

            //var sunLight = sun.GetBehaviour<SunLight>();
            //camera.View = sunLight.shadowViewMatrices[0];
            //camera.Projection = sunLight.shadowProjectionMatrices[0];
            //camera.NearClip = 1;
            //camera.FarClip = sunLight.farClip[0];

            for (int i = 0; i < _lights.Count / 10; i++)
            {
                var light = _lights[i];
                var offset = i * (MathHelper.TwoPi / (_lights.Count / 10f));
                light.Position = new Vector3(
                    (float)Math.Cos(totalTime + offset) * 40,
                    10,
                    (float)Math.Sin(totalTime + offset) * 40);
            }

            for (int i = _lights.Count / 10; i < _lights.Count; i++)
            {
                var light = _lights[i];
                var offset = i * (MathHelper.TwoPi / (_lights.Count  - (_lights.Count / 10f)));
                light.Position = new Vector3(
                    (float)Math.Cos(-totalTime + offset) * 100,
                    10,
                    (float)Math.Sin(-totalTime + offset) * 100);
            }

            //spotLight.Position = new Vector3(
            //    (float)Math.Cos(-totalTime + 5) * 50,
            //    100,
            //    (float)Math.Sin(-totalTime + 5) * 50);
            //spotLight.Direction = Vector3.Normalize(-spotLight.Position);

            _scene.Update((float)gameTime.ElapsedGameTime.TotalSeconds);
        }

        public void Draw(GameTime gameTime)
        {
            _scene.Draw();

            //var sunLight = sun.GetBehaviour<SunLight>();
            //if (spotLight.shadowMap != null)
            //{
            //    sb.Begin(SpriteSortMode.Immediate, BlendState.Opaque, SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullCounterClockwise);
            //    sb.Draw(spotLight.shadowMap, Vector2.Zero, Color.White);
            //    sb.End();
            //}

            //var quad = new Quad(sb.GraphicsDevice);
            //basic.Parameters["Colour"].SetValue(Color.White.ToVector4());
            //quad.Draw(basic);
        }
    }
}
