using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Myre.Debugging.UI;
using Myre.Entities;
using Myre.Graphics.Animation;
using Ninject;
using Microsoft.Xna.Framework;
using Myre.Graphics;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Myre.Graphics.Geometry;
using Myre.Graphics.Lighting;
using Myre.UI;
using Myre.Debugging;
using Myre.UI.InputDevices;
using Microsoft.Xna.Framework.Input;
using Myre.Collections;
using Myre.Graphics.Particles;

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
        }

        public TestScene(IKernel kernel, Game game, ContentManager content, GraphicsDevice device)
            : this(kernel, game, content, device, null)
        {
        }

        public TestScene(IKernel kernel, Game game, ContentManager content, GraphicsDevice device, SceneConfiguration config)
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

            var cameraDesc = kernel.Get<EntityDescription>();
            cameraDesc.AddProperty<Camera>("camera");
            cameraDesc.AddProperty<Viewport>("viewport");
            cameraDesc.AddBehaviour<View>();
            var cameraEntity = cameraDesc.Create();
            cameraEntity.GetProperty<Camera>("camera").Value = camera;
            cameraEntity.GetProperty<Viewport>("viewport").Value = new Viewport() { Width = device.PresentationParameters.BackBufferWidth, Height = device.PresentationParameters.BackBufferHeight };
            scene.Add(cameraEntity);

            if (config.Skybox)
            {
                var skyboxDesc = kernel.Get<EntityDescription>();
                skyboxDesc.AddBehaviour<Skybox>();
                var skybox = skyboxDesc.Create();
                skybox.GetProperty<TextureCube>("texture").Value = content.Load<TextureCube>("StormCubeMap");
                skybox.GetProperty<float>("brightness").Value = 0.5f;
                skybox.GetProperty<bool>("gamma_correct").Value = false;
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
            pointLight.AddProperty<Vector3>("position");
            pointLight.AddProperty<Vector3>("colour");
            pointLight.AddProperty<float>("range");
            pointLight.AddBehaviour<PointLight>();
            //scene.Add(pointLight.Create());

            lights = new List<PointLight>();
            var rng = new Random();
            for (int i = 0; i < config.RandomPointLights; i++)
            {
                var entity = pointLight.Create();
                scene.Add(entity);

                entity.GetProperty<Vector3>("position").Value = new Vector3(0, 10, 0);
                entity.GetProperty<Vector3>("colour").Value = new Vector3(0, 5, 0);
                entity.GetProperty<float>("range").Value = 200;

                var light = entity.GetBehaviour<PointLight>();
                light.Colour = Vector3.Normalize(new Vector3(0.1f + (float)rng.NextDouble(), 0.1f + (float)rng.NextDouble(), 0.1f + (float)rng.NextDouble())) * 10;
                lights.Add(light);
            }

            if (config.Spotlight)
            {
                var spotLight = kernel.Get<EntityDescription>();
                spotLight.AddProperty<Vector3>("position");
                spotLight.AddProperty<Vector3>("colour");
                spotLight.AddProperty<Vector3>("direction");
                spotLight.AddProperty<float>("angle");
                spotLight.AddProperty<float>("range");
                spotLight.AddProperty<Texture2D>("mask");
                spotLight.AddProperty<int>("shadow_resolution");
                spotLight.AddBehaviour<SpotLight>();
                var spotLightEntity = spotLight.Create();
                spotLightEntity.GetProperty<Vector3>("position").Value = new Vector3(-180, 250, 0);
                spotLightEntity.GetProperty<Vector3>("colour").Value = new Vector3(10);
                spotLightEntity.GetProperty<Vector3>("direction").Value = new Vector3(0, -1, 0);
                spotLightEntity.GetProperty<float>("angle").Value = MathHelper.PiOver2;
                spotLightEntity.GetProperty<float>("range").Value = 500;
                spotLightEntity.GetProperty<Texture2D>("mask").Value = content.Load<Texture2D>("Chrysanthemum");
                spotLightEntity.GetProperty<int>("shadow_resolution").Value = 1024;
                this.spotLight = spotLightEntity.GetBehaviour<SpotLight>();
                scene.Add(spotLightEntity);
            }

            if (config.AmbientLight)
            {
                var ambientLight = kernel.Get<EntityDescription>();
                ambientLight.AddProperty<Vector3>("sky_colour");
                ambientLight.AddProperty<Vector3>("ground_colour");
                ambientLight.AddProperty<Vector3>("up");
                ambientLight.AddBehaviour<AmbientLight>();
                var ambientLightEntity = ambientLight.Create();
                ambientLightEntity.GetProperty<Vector3>("sky_colour").Value = new Vector3(0.04f);
                ambientLightEntity.GetProperty<Vector3>("ground_colour").Value = new Vector3(0.04f, 0.05f, 0.04f);
                ambientLightEntity.GetProperty<Vector3>("up").Value = Vector3.Up;
                scene.Add(ambientLightEntity);
            }

            //var floor = content.Load<ModelData>(@"Models\Ground");
            //var floorEntity = kernel.Get<EntityDescription>();
            //floorEntity.AddProperty<ModelData>("model", floor);
            //floorEntity.AddProperty<Matrix>("transform", Matrix.CreateScale(2));
            //floorEntity.AddProperty<bool>("isstatic", true);
            //floorEntity.AddBehaviour<ModelInstance>();
            //scene.Add(floorEntity.Create());

            var lizardModel = content.Load<ModelData>(@"Models\lizard");
            var lizard = kernel.Get<EntityDescription>();
            lizard.AddProperty<ModelData>("model");
            lizard.AddProperty<Matrix>("transform");
            lizard.AddProperty<bool>("is_static");
            var lizardEntity = lizard.Create();
            lizardEntity.GetProperty<ModelData>("model").Value = lizardModel;
            lizardEntity.GetProperty<Matrix>("transform").Value = Matrix.CreateScale(50 / 700f) * Matrix.CreateTranslation(150, 0, 0);
            lizardEntity.GetProperty<bool>("is_static").Value = true;
            lizard.AddBehaviour<ModelInstance>();

            scene.Add(lizardEntity);

            //var ship1 = content.Load<ModelData>(@"Models\Ship1");
            //var ship1Entity = kernel.Get<EntityDescription>();
            //ship1Entity.AddProperty<ModelData>("model", ship1);
            //ship1Entity.AddProperty<Matrix>("transform", Matrix.CreateTranslation(30, 0, 0));
            //ship1Entity.AddProperty<bool>("is_static", true);
            //ship1Entity.AddBehaviour<ModelInstance>();
            //scene.Add(ship1Entity.Create());

            var hebeModel = content.Load<ModelData>(@"Models\Hebe2");
            var hebe = kernel.Get<EntityDescription>();
            hebe.AddProperty<ModelData>("model");
            hebe.AddProperty<Matrix>("transform");
            hebe.AddProperty<bool>("is_static");
            hebe.AddBehaviour<ModelInstance>();
            //var hebeEntity = hebe.Create();
            //hebeEntity.GetProperty<ModelData>("model").Value = hebeModel;
            //hebeEntity.GetProperty<Matrix>("transform").Value = Matrix.CreateScale(25 / hebeModel.Meshes.First().BoundingSphere.Radius)
            //                                                        * Matrix.CreateRotationY(MathHelper.PiOver2)
            //                                                        * Matrix.CreateTranslation(-150, 20, 0);
            //hebeEntity.GetProperty<bool>("is_static").Value = true;
            //scene.Add(hebeEntity);

            var dudeModel = content.Load<ModelData>(@"dude");
            var dude = kernel.Get<EntityDescription>();
            dude.AddProperty<ModelData>("model", dudeModel);
            dude.AddProperty<Matrix>("transform", Matrix.CreateScale(0.75f) * Matrix.CreateTranslation(-50, 0, 0));
            dude.AddProperty<bool>("is_static", true);
            dude.AddBehaviour<ModelInstance>();
            dude.AddBehaviour<Animated>();
            var dudeEntity = dude.Create();
            scene.Add(dudeEntity);

            var lightBlocker = hebe.Create();
            hebeTransform = lightBlocker.GetProperty<Matrix>("transform");
            lightBlocker.GetProperty<ModelData>("model").Value = hebeModel;
            lightBlocker.GetProperty<Matrix>("transform").Value = Matrix.CreateScale(25 / hebeModel.Meshes.First().BoundingSphere.Radius)
                                                                    * Matrix.CreateRotationY(MathHelper.PiOver2)
                                                                    * Matrix.CreateTranslation(-150, 20, 0);
            lightBlocker.GetProperty<bool>("is_static").Value = true;
            scene.Add(lightBlocker);

            var sponzaModel = content.Load<ModelData>(@"Sponza");
            var sponza = kernel.Get<EntityDescription>();
            sponza.AddProperty<ModelData>("model");
            sponza.AddProperty<Matrix>("transform");
            sponza.AddProperty<bool>("is_static");
            sponza.AddBehaviour<ModelInstance>();
            var sponzaEntity = sponza.Create();
            sponzaEntity.GetProperty<ModelData>("model").Value = sponzaModel;
            sponzaEntity.GetProperty<Matrix>("transform").Value = Matrix.Identity * Matrix.CreateScale(1);
            sponzaEntity.GetProperty<bool>("is_static").Value = true;
            scene.Add(sponzaEntity);

            var renderer = scene.GetService<Renderer>();
            resolution = renderer.Data.Get<Vector2>("resolution");

            var console = kernel.Get<CommandConsole>();
            renderer.Settings.BindCommandEngine(console.Engine);

            if (config.Fire)
            {
                var fire1 = Fire.Create(kernel, content, new Vector3(123.5f, 30f, -55f));
                var fire2 = Fire.Create(kernel, content, new Vector3(123.5f, 30f, 35f));
                var fire3 = Fire.Create(kernel, content, new Vector3(-157f, 30f, 35f));
                var fire4 = Fire.Create(kernel, content, new Vector3(-157f, 30f, -55f));

                scene.Add(fire1);
                scene.Add(fire2);
                scene.Add(fire3);
                scene.Add(fire4);
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
                var offset = i * (MathHelper.TwoPi / (lights.Count / 10));
                light.Position = new Vector3(
                    (float)Math.Cos(totalTime + offset) * 40,
                    10,
                    (float)Math.Sin(totalTime + offset) * 40);
            }

            for (int i = lights.Count / 10; i < lights.Count; i++)
            {
                var light = lights[i];
                var offset = i * (MathHelper.TwoPi / (lights.Count  - (lights.Count / 10)));
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
