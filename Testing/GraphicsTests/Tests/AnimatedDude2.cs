using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Myre;
using Myre.Entities;
using Myre.Graphics;
using Myre.Graphics.Animation;
using Myre.Graphics.Animation.Clips;
using Myre.Graphics.Deferred;
using Myre.Graphics.Geometry;
using Myre.Graphics.Lighting;
using Myre.Graphics.Translucency;
using Ninject;
using System;
using System.Numerics;
using Myre.Extensions;
using GameTime = Microsoft.Xna.Framework.GameTime;
using MathHelper = Microsoft.Xna.Framework.MathHelper;

namespace GraphicsTests.Tests
{
    class AnimatedDude2
        : TestScreen
    {
        private readonly IKernel _kernel;
        private readonly ContentManager _content;
        private readonly GraphicsDevice _device;

        private Scene _scene;
        private ModelInstance _dude;
        private AnimationPlayer _animationPlayer;

        private readonly string[] _sequence = {
            //"Walk-Forward-Stand", "Walk-Forward-Stand", "Walk-Forward-Stand", "Walk-Forward-Stand", "Walk-Forward-Stand",

        };

        public AnimatedDude2(IKernel kernel, ContentManager content, GraphicsDevice device)
            : base("Animated Dude 2", kernel)
        {
            _kernel = kernel;
            _content = content;
            _device = device;
        }

        protected override void BeginTransitionOn()
        {
            _scene = _kernel.Get<Scene>();

            var model = _content.Load<ModelData>(@"models/BlockDude");
            var dude = _kernel.Get<EntityDescription>();
            dude.AddProperty(new TypedName<ModelData>("model"), model);
            dude.AddProperty(new TypedName<Matrix4x4>("transform"), Matrix4x4.CreateScale(20f) * Matrix4x4.CreateTranslation(0, 0, 175));
            dude.AddProperty(new TypedName<bool>("is_static"), false);
            dude.AddBehaviour<ModelInstance>();
            dude.AddBehaviour<Animated>();
            dude.AddBehaviour<AnimationPlayer>();
            var dudeEntity = dude.Create();
            _scene.Add(dudeEntity);
            _animationPlayer = dudeEntity.GetBehaviour<AnimationPlayer>(null);
            _animationPlayer.EnableRootBoneTranslationY = false;
            _animationPlayer.EnableRootBoneTranslationX = false;
            _animationPlayer.EnableRootBoneTranslationZ = false;
            _animationPlayer.EnableRootBoneScale = false;

            _dude = dudeEntity.GetBehaviour<ModelInstance>(null);

            _animationPlayer.DefaultClip = new AnimationPlayer.ClipPlaybackParameters
            {
                Clip = _content.Load<Clip>("Models/DudeAnimations/Idle-Stand"),
                FadeInTime = TimeSpan.FromSeconds(0.15f),
                FadeOutTime = TimeSpan.FromSeconds(0.15f),
                Loop = true,
            };

            foreach (var name in _sequence)
            {
                _animationPlayer.EnqueueClip(new AnimationPlayer.ClipPlaybackParameters
                {
                    Clip = new TimeScaleClip(_content.Load<Clip>("Models/DudeAnimations/" + name), 1f),
                    FadeInTime = TimeSpan.FromSeconds(0.1f),
                    FadeOutTime = TimeSpan.FromSeconds(0.0f),
                });
            }

            var camera = new Camera { NearClip = 1, FarClip = 7000, View = Matrix4x4.CreateLookAt(new Vector3(100, 50, -200), new Vector3(0, 20, 0), Vector3.UnitY) };
            camera.Projection = Matrix4x4.CreatePerspectiveFieldOfView(MathHelper.ToRadians(60), 16f / 9f, camera.NearClip, camera.FarClip);
            var cameraDesc = _kernel.Get<EntityDescription>();
            cameraDesc.AddProperty(new TypedName<Camera>("camera"));
            cameraDesc.AddProperty(new TypedName<Viewport>("viewport"));
            cameraDesc.AddBehaviour<View>();
            var cameraEntity = cameraDesc.Create();
            cameraEntity.GetProperty(new TypedName<Camera>("camera")).Value = camera;
            cameraEntity.GetProperty(new TypedName<Viewport>("viewport")).Value = new Viewport() { Width = _device.PresentationParameters.BackBufferWidth, Height = _device.PresentationParameters.BackBufferHeight };
            _scene.Add(cameraEntity);

            var ambientLight = _kernel.Get<EntityDescription>();
            ambientLight.AddProperty(new TypedName<Vector3>("sky_colour"), new Vector3(0.44f, 0.44f, 0.74f));
            ambientLight.AddProperty(new TypedName<Vector3>("ground_colour"), new Vector3(0.24f, 0.35f, 0.24f));
            ambientLight.AddProperty(new TypedName<Vector3>("up"), Vector3.UnitY);
            ambientLight.AddBehaviour<AmbientLight>();
            _scene.Add(ambientLight.Create());

            var sponza = _kernel.Get<EntityDescription>();
            sponza.AddProperty(new TypedName<ModelData>("model"), _content.Load<ModelData>(@"Sponza"));
            sponza.AddProperty(new TypedName<Matrix4x4>("transform"), Matrix4x4.CreateScale(0.5f) * Matrix4x4.CreateTranslation(-350, 0, 0));
            sponza.AddProperty(new TypedName<bool>("is_static"), true);
            sponza.AddBehaviour<ModelInstance>();
            _scene.Add(sponza.Create());

            var spotLight = _kernel.Get<EntityDescription>();
            spotLight.AddProperty(new TypedName<Vector3>("position"), new Vector3(150, 50, -50));
            spotLight.AddProperty(new TypedName<Vector3>("colour"), new Vector3(1));
            spotLight.AddProperty(new TypedName<Vector3>("direction"), new Vector3(-1, 0, 0.25f));
            spotLight.AddProperty(new TypedName<float>("angle"), MathHelper.PiOver2);
            spotLight.AddProperty(new TypedName<float>("range"), 1000);
            spotLight.AddProperty(new TypedName<int>("shadow_resolution"), 1024);
            spotLight.AddBehaviour<SpotLight>();
            var spotLightEntity = spotLight.Create();
            _scene.Add(spotLightEntity);

            _scene.GetService<Renderer>().StartPlan()
                  .Then<GeometryBufferComponent>()
                  .Then<EdgeDetectComponent>()
                  .Then<Ssao>()
                  .Then<LightingComponent>()
                  .Then<TranslucentComponent>()
                  .Then<ToneMapComponent>()
                  .Then<AntiAliasComponent>()
                  .Apply();

            base.OnShown();
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            _scene.Update((float)gameTime.ElapsedGameTime.TotalSeconds);
            //_dude.Transform *= Matrix4x4.CreateTranslation(50 * new Vector3(_animationPlayer.RootBoneTransfomationDelta.Translation.X, 0, _animationPlayer.RootBoneTransfomationDelta.Translation.Z));
            //_dude.Transform = Matrix4x4.CreateFromAxisAngle(Vector3.UnitY, (float)gameTime.TotalSeconds() / 4) * Matrix4x4.CreateScale(20f) * Matrix4x4.CreateTranslation(0, 0, 175);
        }

        public override void Draw(GameTime gameTime)
        {
            _scene.Draw();
            base.Draw(gameTime);
        }
    }
}
