using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Myre.Entities;
using Myre.Graphics;
using Myre.Graphics.Animation;
using Myre.Graphics.Animation.Clips;
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
        private readonly ModelInstance _dude;
        private readonly Animated _animation;

        private readonly string[] _sequence = new string[]
        {
            "walk01","walk01","walk01","walk03","walk03",
            "death02", "get-up",
            "run01","run02","run03","run01",
            "run-jump","roll-forward","run01",
            "run_ready-shoot","run_ready-shoot",
            "run_shooting","run_shooting", "run_shooting",
            "firing01","firing02","firing03","firing03","walk_shooting",
            "walk01", "walk02", "walk01", "walk03",
            "emo03", "walk03", "chat01"
        };

        public AnimatedDude(IKernel kernel, ContentManager content, GraphicsDevice device)
            : base("Animated Dude", kernel)
        {
            _scene = kernel.Get<Scene>();

            var model = content.Load<ModelData>(@"models/zoe_fbx");
            var dude = kernel.Get<EntityDescription>();
            dude.AddProperty<ModelData>("model", model);
            dude.AddProperty<Matrix>("transform", Matrix.Identity);
            dude.AddProperty<bool>("is_static", true);
            dude.AddBehaviour<ModelInstance>();
            dude.AddBehaviour<Animated>();
            var dudeEntity = dude.Create();
            _scene.Add(dudeEntity);
            _animation = dudeEntity.GetBehaviour<Animated>();
            _animation.EnableRootBoneTranslationY = true;
            _animation.EnableRootBoneTranslationX = false;
            _animation.EnableRootBoneTranslationZ = false;

            _dude = dudeEntity.GetBehaviour<ModelInstance>();

            _animation.DefaultClip = new Animated.ClipPlaybackParameters
            {
                Clip = new RandomClip(
                    content.Load<Clip>("Models/ZoeAnimations/idle02"),
                    content.Load<Clip>("Models/ZoeAnimations/idle01")
                    ),
                FadeInTime = TimeSpan.FromSeconds(1f),
                FadeOutTime = TimeSpan.FromSeconds(0.5f),
                Loop = false,
            };

            foreach (var name in _sequence)
            {
                _animation.EnqueueClip(new Animated.ClipPlaybackParameters
                {
                    Clip = content.Load<Clip>("Models/ZoeAnimations/" + name),
                    FadeInTime = TimeSpan.FromSeconds(0.1f),
                    FadeOutTime = TimeSpan.FromSeconds(0.0f),
                    Loop = false,
                });
            }

            var camera = new Camera { NearClip = 1, FarClip = 7000, View = Matrix.CreateTranslation(-20, -40, 0) * Matrix.CreateLookAt(new Vector3(0, 0, -200), new Vector3(0, 0, 0), Vector3.Up) };
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
            ambientLight.AddProperty<Vector3>("sky_colour", new Vector3(0.44f));
            ambientLight.AddProperty<Vector3>("ground_colour", new Vector3(0.24f, 0.35f, 0.24f));
            ambientLight.AddProperty<Vector3>("up", Vector3.Up);
            ambientLight.AddBehaviour<AmbientLight>();
            _scene.Add(ambientLight.Create());

            var sponza = kernel.Get<EntityDescription>();
            sponza.AddProperty<ModelData>("model", content.Load<ModelData>(@"Sponza"));
            sponza.AddProperty<Matrix>("transform", Matrix.CreateScale(0.5f) * Matrix.CreateTranslation(-350, 0, 0));
            sponza.AddProperty<bool>("is_static", true);
            sponza.AddBehaviour<ModelInstance>();
            _scene.Add(sponza.Create());

            var spotLight = kernel.Get<EntityDescription>();
            spotLight.AddProperty<Vector3>("position", new Vector3(150, 50, 0));
            spotLight.AddProperty<Vector3>("colour", new Vector3(5));
            spotLight.AddProperty<Vector3>("direction", new Vector3(-1, 0, 0));
            spotLight.AddProperty<float>("angle", MathHelper.PiOver2);
            spotLight.AddProperty<float>("range", 1000);
            spotLight.AddProperty<Texture2D>("mask", content.Load<Texture2D>("Chrysanthemum"));
            spotLight.AddProperty<int>("shadow_resolution", 1024);
            spotLight.AddBehaviour<SpotLight>();
            var spotLightEntity = spotLight.Create();
            _scene.Add(spotLightEntity);

            _scene.GetService<Renderer>().StartPlan()
                  .Then<GeometryBufferComponent>()
                  .Then<EdgeDetectComponent>()
                  .Then<Ssao>()
                  .Then<LightingComponent>()
                //.Then<ToneMapComponent>()
                  .Then<TranslucentComponent>()
                  .Apply();
        }

        public override void Update(GameTime gameTime)
        {
            _scene.Update((float)gameTime.ElapsedGameTime.TotalSeconds);
            base.Update(gameTime);

            var anim = _dude.Owner.GetBehaviour<Animated>();

            Vector3 position, scale;
            Quaternion rotation;
            anim.RootBoneTransfomation.Decompose(out scale, out rotation, out position);

            _dude.Transform = Matrix.CreateTranslation(new Vector3(position.X, 0, 0 * position.Z));
        }

        public override void Draw(GameTime gameTime)
        {
            _scene.Draw();
            base.Draw(gameTime);
        }
    }
}
