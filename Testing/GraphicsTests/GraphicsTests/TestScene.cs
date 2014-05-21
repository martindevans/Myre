using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
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

namespace GraphicsTests
{
    class TestScene
    {
        Game game;
        Scene scene;
        Camera camera;
        List<PointLight> lights;
        SpotLight spotLight;
        Entity sun;
        Box<Vector2> resolution;
        SpriteBatch sb;
        Effect basic;
        Property<Matrix> hebeTransform;
        bool paused;
        KeyboardState previousKeyboard;

        Vector3 cameraPosition;
        Vector3 cameraRotation;
        CameraScript cameraScript;

        private ModelInstance _sponza;

        public Scene Scene
        {
            get { return scene; }
        }

        public Camera Camera
        {
            get { return camera; }
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

        public TestScene(IKernel kernel, Game game, ContentManager content, GraphicsDevice device, SceneConfiguration config = null)
        {
            config = config ?? new SceneConfiguration();

            scene = new Scene(kernel);
            this.game = game;

            sb = new SpriteBatch(device);
            basic = content.Load<Effect>("Basic");

            cameraPosition = new Vector3(100, 50, 0);
            
            camera = new Camera();
            camera.NearClip = 1;
            camera.FarClip = 700;
            camera.View = Matrix.CreateLookAt(cameraPosition, new Vector3(0, 50, 0), Vector3.Up);
            camera.Projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(60), 16f / 9f, camera.NearClip, camera.FarClip);

            if (config.View)
            {
                var cameraDesc = kernel.Get<EntityDescription>();
                cameraDesc.AddProperty(new TypedName<Camera>("camera"));
                cameraDesc.AddProperty(new TypedName<Viewport>("viewport"));
                cameraDesc.AddBehaviour<View>();
                var cameraEntity = cameraDesc.Create();
                cameraEntity.GetProperty(new TypedName<Camera>("camera")).Value = camera;
                cameraEntity.GetProperty(new TypedName<Viewport>("viewport")).Value = new Viewport() {Width = device.PresentationParameters.BackBufferWidth, Height = device.PresentationParameters.BackBufferHeight};
                scene.Add(cameraEntity);
            }

            if (config.Skybox)
            {
                var skyboxDesc = kernel.Get<EntityDescription>();
                skyboxDesc.AddBehaviour<Skybox>();
                var skybox = skyboxDesc.Create();
                skybox.GetProperty(new TypedName<TextureCube>("texture")).Value = content.Load<TextureCube>("StormCubeMap");
                skybox.GetProperty(new TypedName<float>("brightness")).Value = 0.5f;
                skybox.GetProperty(new TypedName<bool>("gamma_correct")).Value = false;
                scene.Add(skybox);
            }

            //var sunEntity = kernel.Get<EntityDescription>();
            //sunEntity.AddProperty<Vector3>("direction", Vector3.Normalize(new Vector3(-.2f, -1f, .3f)));
            //sunEntity.AddProperty<Vector3>("colour", new Vector3(5f));
            //sunEntity.AddProperty<int>("shadow_resolution", 4096);
            //sunEntity.AddBehaviour<SunLight>();
            //sun = sunEntity.Create();
            //scene.Add(sun);

            //var sun2 = sunEntity.Create();
            //sun2.GetProperty<Vector3>("direction").Value = Vector3.Normalize(new Vector3(1, -1, 0));
            //sun2.GetProperty<Vector3>("colour").Value = new Vector3(1, 0, 0);
            //scene.Add(sun2);

            var pointLight = kernel.Get<EntityDescription>();
            pointLight.AddProperty(new TypedName<Vector3>("position"));
            pointLight.AddProperty(new TypedName<Vector3>("colour"));
            pointLight.AddProperty(new TypedName<float>("range"));
            pointLight.AddBehaviour<PointLight>();
            //scene.Add(pointLight.Create());

            lights = new List<PointLight>();
            var rng = new Random();
            for (int i = 0; i < config.RandomPointLights; i++)
            {
                var entity = pointLight.Create();
                scene.Add(entity);

                entity.GetProperty(new TypedName<Vector3>("position")).Value = new Vector3(0, 10, 0);
                entity.GetProperty(new TypedName<Vector3>("colour")).Value = new Vector3(0, 5, 0);
                entity.GetProperty(new TypedName<float>("range")).Value = 200;

                var light = entity.GetBehaviour<PointLight>();
                light.Colour = Vector3.Normalize(new Vector3(0.1f + (float)rng.NextDouble(), 0.1f + (float)rng.NextDouble(), 0.1f + (float)rng.NextDouble())) * 10;
                lights.Add(light);
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
                this.spotLight = spotLightEntity.GetBehaviour<SpotLight>();
                scene.Add(spotLightEntity);
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
                ambientLightEntity.GetProperty(new TypedName<Vector3>("up")).Value = Vector3.Up;
                scene.Add(ambientLightEntity);
            }

            if (config.SunLight)
            {
                var sunlight = kernel.Get<EntityDescription>();
                sunlight.AddBehaviour<SunLight>();
                var sunlightEntity = sunlight.Create();
                sunlightEntity.GetProperty(new TypedName<Vector3>("colour")).Value = new Vector3(1, 0.75f, 0.6f);
                sunlightEntity.GetProperty(new TypedName<Vector3>("direction")).Value = Vector3.Down;
                sunlightEntity.GetProperty(new TypedName<int>("shadow_resolution")).Value = 1024;
                scene.Add(sunlightEntity);
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

            var hebeModel = content.Load<ModelData>(@"Models\Hebe2");
            var hebe = kernel.Get<EntityDescription>();
            hebe.AddProperty(new TypedName<ModelData>("model"));
            hebe.AddProperty(new TypedName<Matrix>("transform"));
            hebe.AddProperty(new TypedName<bool>("is_static"));
            hebe.AddBehaviour<ModelInstance>();
            var hebeEntity = hebe.Create();
            hebeEntity.GetProperty(new TypedName<ModelData>("model")).Value = hebeModel;
            hebeEntity.GetProperty(new TypedName<Matrix>("transform")).Value = Matrix.CreateScale(25 / hebeModel.Meshes.First().BoundingSphere.Radius)
                                                                    * Matrix.CreateRotationY(MathHelper.PiOver2)
                                                                    * Matrix.CreateTranslation(-150, 20, 0);
            hebeEntity.GetProperty(new TypedName<bool>("is_static")).Value = true;
            hebeEntity.GetProperty(ModelInstance.OpacityName).Value = 0.05f;
            scene.Add(hebeEntity);

            //var dudeModel = content.Load<ModelData>(@"dude");
            //var dude = kernel.Get<EntityDescription>();
            //dude.AddProperty<ModelData>("model", dudeModel);
            //dude.AddProperty<Matrix>("transform", Matrix.CreateScale(0.75f) * Matrix.CreateTranslation(-50, 0, 0));
            //dude.AddProperty<bool>("is_static", true);
            //dude.AddBehaviour<ModelInstance>();
            //dude.AddBehaviour<Animated>();
            //var dudeEntity = dude.Create();
            //scene.Add(dudeEntity);

            var lightBlocker = hebe.Create();
            hebeTransform = lightBlocker.GetProperty(new TypedName<Matrix>("transform"));
            lightBlocker.GetProperty(new TypedName<ModelData>("model")).Value = hebeModel;
            lightBlocker.GetProperty(new TypedName<Matrix>("transform")).Value = Matrix.CreateScale(25 / hebeModel.Meshes.First().BoundingSphere.Radius)
                                                                    * Matrix.CreateRotationY(MathHelper.PiOver2)
                                                                    * Matrix.CreateTranslation(-150, 20, 0);
            lightBlocker.GetProperty(new TypedName<bool>("is_static")).Value = false;
            lightBlocker.GetProperty(ModelInstance.OpacityName).Value = 0.05f;
            scene.Add(lightBlocker);

            var sponzaModel = content.Load<ModelData>(@"Sponza");
            var sponza = kernel.Get<EntityDescription>();
            sponza.AddProperty(new TypedName<ModelData>("model"));
            sponza.AddProperty(new TypedName<Matrix>("transform"));
            sponza.AddProperty(new TypedName<bool>("is_static"));
            sponza.AddBehaviour<ModelInstance>();
            var sponzaEntity = sponza.Create();
            sponzaEntity.GetProperty(new TypedName<ModelData>("model")).Value = sponzaModel;
            sponzaEntity.GetProperty(new TypedName<Matrix>("transform")).Value = Matrix.Identity * Matrix.CreateScale(1);
            sponzaEntity.GetProperty(new TypedName<bool>("is_static")).Value = true;
            scene.Add(sponzaEntity);
            _sponza = sponzaEntity.GetBehaviour<ModelInstance>();

            var renderer = scene.GetService<Renderer>();
            resolution = renderer.Data.Get<Vector2>("resolution");

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

            cameraScript = new CameraScript(camera);
            cameraScript.AddWaypoint(0, new Vector3(218, 160, 104), new Vector3(0, 150, 0));
            cameraScript.AddWaypoint(10, new Vector3(-195, 160, 104), new Vector3(-150, 150, 0));
            cameraScript.AddWaypoint(12, new Vector3(-270, 160, 96), new Vector3(-150, 150, 0));
            cameraScript.AddWaypoint(14, new Vector3(-302, 160, 45), new Vector3(-150, 150, 0));
            cameraScript.AddWaypoint(16, new Vector3(-286, 160, 22), new Vector3(-150, 150, 0));
            cameraScript.AddWaypoint(18, new Vector3(-276, 160, 22), new Vector3(-150, 100, 0));
            cameraScript.AddWaypoint(20, new Vector3(-158, 42, 19), new Vector3(-150, 40, 0));
            cameraScript.AddWaypoint(21, new Vector3(-105, 24, -7), new Vector3(-150, 40, 0));
            cameraScript.AddWaypoint(23, new Vector3(-105, 44, -7), new Vector3(-150, 40, 0));
            cameraScript.AddWaypoint(27, new Vector3(-105, 50, -7), new Vector3(-80, 50, -100));
            cameraScript.AddWaypoint(32, new Vector3(100, 50, -7), new Vector3(150, 40, 0));
            cameraScript.AddWaypoint(34, new Vector3(100, 50, -7), new Vector3(150, 40, 100));
            cameraScript.AddWaypoint(36, new Vector3(100, 50, -7), new Vector3(0, 60, 0));
            //cameraScript.AddWaypoint(1000, new Vector3(100, 50, -7), new Vector3(0, 60, 0));
            cameraScript.Initialise();
        }

        public void Update(GameTime gameTime)
        {
            var totalTime = (float)gameTime.TotalGameTime.TotalSeconds / 2;
            var time = (float)gameTime.ElapsedGameTime.TotalSeconds;

            MouseState mouse = Mouse.GetState();
            KeyboardState keyboard = Keyboard.GetState();

            spotLight.Direction = Vector3.Normalize(Vector3.Lerp(Vector3.Normalize(new Vector3(-0.5f, -1, 0)), Vector3.Normalize(new Vector3(0.5f, -1, 0)), (float)Math.Sin(totalTime) * 0.5f + 0.5f));

            game.IsMouseVisible = false;
            if (mouse.IsButtonDown(MouseButtons.Right))
            {
                var mousePosition = new Vector2(mouse.X, mouse.Y);
                var mouseDelta = mousePosition - resolution.Value / 2;

                cameraRotation.Y -= mouseDelta.X * time * 0.1f;
                cameraRotation.X -= mouseDelta.Y * time * 0.1f;

                var rotation = Matrix.CreateFromYawPitchRoll(cameraRotation.Y, cameraRotation.X, cameraRotation.Z);
                var forward = Vector3.TransformNormal(Vector3.Forward, rotation);
                var right = Vector3.TransformNormal(Vector3.Right, rotation);

                forward.Normalize();
                right.Normalize();

                if (keyboard.IsKeyDown(Keys.W))
                    cameraPosition += forward * time * 50;
                if (keyboard.IsKeyDown(Keys.S))
                    cameraPosition -= forward * time * 50f;
                if (keyboard.IsKeyDown(Keys.A))
                    cameraPosition -= right * time * 50f;
                if (keyboard.IsKeyDown(Keys.D))
                    cameraPosition += right * time * 50f;

                camera.View = Matrix.Invert(rotation * Matrix.CreateTranslation(cameraPosition));

                Mouse.SetPosition((int)resolution.Value.X / 2, (int)resolution.Value.Y / 2);
                //camera.View = Matrix.CreateLookAt(new Vector3(0, 60, -7), new Vector3(50, 30, -50), Vector3.Up);
            }
            else
            {
                if (keyboard.IsKeyDown(Keys.Space) && previousKeyboard.IsKeyUp(Keys.Space))
                    paused = !paused;

                if (!paused)
                    cameraScript.Update((float)gameTime.ElapsedGameTime.TotalSeconds);

                cameraPosition = cameraScript.Position;
            }

            previousKeyboard = keyboard;

            //camera.View = spotLight.view;
            //camera.Projection = spotLight.projection;

            //var sunLight = sun.GetBehaviour<SunLight>();
            //camera.View = sunLight.shadowViewMatrices[0];
            //camera.Projection = sunLight.shadowProjectionMatrices[0];
            //camera.NearClip = 1;
            //camera.FarClip = sunLight.farClip[0];

            for (int i = 0; i < lights.Count / 10; i++)
            {
                var light = lights[i];
                var offset = i * (MathHelper.TwoPi / (lights.Count / 10f));
                light.Position = new Vector3(
                    (float)Math.Cos(totalTime + offset) * 40,
                    10,
                    (float)Math.Sin(totalTime + offset) * 40);
            }

            for (int i = lights.Count / 10; i < lights.Count; i++)
            {
                var light = lights[i];
                var offset = i * (MathHelper.TwoPi / (lights.Count  - (lights.Count / 10f)));
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

            hebeTransform.Value = Matrix.CreateRotationX(MathHelper.PiOver2)
                                * Matrix.CreateScale(0.1f)
                                * Matrix.CreateRotationY((float)gameTime.TotalGameTime.TotalSeconds)
                                * Matrix.CreateTranslation(new Vector3(-180, 230, 0));

            scene.Update((float)gameTime.ElapsedGameTime.TotalSeconds);
        }

        public void Draw(GameTime gameTime)
        {
            scene.Draw();

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
