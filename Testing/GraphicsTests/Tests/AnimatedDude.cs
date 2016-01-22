//using Microsoft.Xna.Framework.Content;
//using Microsoft.Xna.Framework.Graphics;
//using Myre;
//using Myre.Entities;
//using Myre.Graphics;
//using Myre.Graphics.Animation;
//using Myre.Graphics.Animation.Clips;
//using Myre.Graphics.Deferred;
//using Myre.Graphics.Geometry;
//using Myre.Graphics.Lighting;
//using Myre.Graphics.Translucency;
//using Ninject;
//using System;
//using System.Numerics;

//using GameTime = Microsoft.Xna.Framework.GameTime;
//using MathHelper = Microsoft.Xna.Framework.MathHelper;

//namespace GraphicsTests.Tests
//{
//    class AnimatedDude
//        : TestScreen
//    {
//        private readonly Scene _scene;
//        private readonly ModelInstance _dude;
//        private readonly AnimationQueue _animationQueue;

//        private readonly string[] _sequence = {
//            "walk-forward-0", "walk-forward-0", "walk-forward-0"
//            //"idle01", "idle02", "jump", "roll-backward-0", "roll-forward-0",
//            //"roll-left-0",
//            //"roll-right-0",
//            //"run-forward-0", "run-forward-1", "run-forward-2",
//            //"run-forward_jump-0", "sitting", "strafe-left-0", "strafe-right-0", "swim-forward-0", "walk-backward-0", "walk-forward-0", "walk-forward-1", "walk-forward-2"
//            //"t-pose"

//        };

//        public AnimatedDude(IKernel kernel, ContentManager content, GraphicsDevice device)
//            : base("Animated Dude", kernel)
//        {
//            _scene = kernel.Get<Scene>();

//            var model = content.Load<ModelData>(@"models/zoe");
//            var dude = kernel.Get<EntityDescription>();
//            dude.AddProperty(new TypedName<ModelData>("model"), model);
//            dude.AddProperty(new TypedName<Matrix4x4>("transform"), Matrix4x4.CreateScale(50f) * Matrix4x4.CreateTranslation(0, 0, -150));
//            dude.AddProperty(new TypedName<bool>("is_static"), false);
//            dude.AddBehaviour<ModelInstance>();
//            dude.AddBehaviour<Animated>();
//            dude.AddBehaviour<AnimationQueue>();
//            var dudeEntity = dude.Create();
//            _scene.Add(dudeEntity);
//            _animationQueue = dudeEntity.GetBehaviour<AnimationQueue>(null);
//            _animationQueue.EnableRootBoneTranslationY = false;
//            _animationQueue.EnableRootBoneTranslationX = false;
//            _animationQueue.EnableRootBoneTranslationZ = false;
//            _animationQueue.EnableRootBoneScale = false;

//            _dude = dudeEntity.GetBehaviour<ModelInstance>(null);

//            _animationQueue.DefaultClip = new AnimationQueue.ClipPlaybackParameters
//            {
//                Clip = content.Load<Clip>("Models/ZoeAnimations/t-pose"),
//                FadeInTime = TimeSpan.FromSeconds(0.25f),
//                FadeOutTime = TimeSpan.FromSeconds(0.25f),
//                Loop = true,
//            };

//            foreach (var name in _sequence)
//            {
//                _animationQueue.EnqueueClip(new AnimationQueue.ClipPlaybackParameters
//                {
//                    Clip = content.Load<Clip>("Models/ZoeAnimations/" + name),
//                    FadeInTime = TimeSpan.FromSeconds(0.1f),
//                    FadeOutTime = TimeSpan.FromSeconds(0.0f),
//                    Loop = false,
//                });
//            }

//            var camera = new Camera { NearClip = 1, FarClip = 7000, View = Matrix4x4.CreateLookAt(new Vector3(100, 50, -200), new Vector3(0, 20, 0), Vector3.UnitY) };
//            camera.Projection = Matrix4x4.CreatePerspectiveFieldOfView(MathHelper.ToRadians(60), 16f / 9f, camera.NearClip, camera.FarClip);
//            var cameraDesc = kernel.Get<EntityDescription>();
//            cameraDesc.AddProperty(new TypedName<Camera>("camera"));
//            cameraDesc.AddProperty(new TypedName<Viewport>("viewport"));
//            cameraDesc.AddBehaviour<View>();
//            var cameraEntity = cameraDesc.Create();
//            cameraEntity.GetProperty(new TypedName<Camera>("camera")).Value = camera;
//            cameraEntity.GetProperty(new TypedName<Viewport>("viewport")).Value = new Viewport() { Width = device.PresentationParameters.BackBufferWidth, Height = device.PresentationParameters.BackBufferHeight };
//            _scene.Add(cameraEntity);

//            var ambientLight = kernel.Get<EntityDescription>();
//            ambientLight.AddProperty(new TypedName<Vector3>("sky_colour"), new Vector3(0.44f, 0.44f, 0.74f));
//            ambientLight.AddProperty(new TypedName<Vector3>("ground_colour"), new Vector3(0.24f, 0.35f, 0.24f));
//            ambientLight.AddProperty(new TypedName<Vector3>("up"), Vector3.UnitY);
//            ambientLight.AddBehaviour<AmbientLight>();
//            _scene.Add(ambientLight.Create());

//            var sponza = kernel.Get<EntityDescription>();
//            sponza.AddProperty(new TypedName<ModelData>("model"), content.Load<ModelData>(@"Sponza"));
//            sponza.AddProperty(new TypedName<Matrix4x4>("transform"), Matrix4x4.CreateScale(0.5f) * Matrix4x4.CreateTranslation(-350, 0, 0));
//            sponza.AddProperty(new TypedName<bool>("is_static"), true);
//            sponza.AddBehaviour<ModelInstance>();
//            _scene.Add(sponza.Create());

//            var spotLight = kernel.Get<EntityDescription>();
//            spotLight.AddProperty(new TypedName<Vector3>("position"), new Vector3(150, 50, -50));
//            spotLight.AddProperty(new TypedName<Vector3>("colour"), new Vector3(1));
//            spotLight.AddProperty(new TypedName<Vector3>("direction"), new Vector3(-1, 0, 0.25f));
//            spotLight.AddProperty(new TypedName<float>("angle"), MathHelper.PiOver2);
//            spotLight.AddProperty(new TypedName<float>("range"), 1000);
//            spotLight.AddProperty(new TypedName<int>("shadow_resolution"), 1024);
//            spotLight.AddBehaviour<SpotLight>();
//            var spotLightEntity = spotLight.Create();
//            _scene.Add(spotLightEntity);

//            _scene.GetService<Renderer>().StartPlan()
//                  .Then<GeometryBufferComponent>()
//                  .Then<EdgeDetectComponent>()
//                  .Then<Ssao>()
//                  .Then<LightingComponent>()
//                  .Then<TranslucentComponent>()
//                  .Then<ToneMapComponent>()
//                  .Then<AntiAliasComponent>()
//                  .Apply();
//        }

//        public override void Update(GameTime gameTime)
//        {
//            base.Update(gameTime);

//            _scene.Update((float)gameTime.ElapsedGameTime.TotalSeconds);
//            _dude.Transform *= Matrix4x4.CreateTranslation(50 * new Vector3(_animationQueue.RootBoneTransfomationDelta.Translation.X, 0, _animationQueue.RootBoneTransfomationDelta.Translation.Z));
//        }

//        public override void Draw(GameTime gameTime)
//        {
//            _scene.Draw();
//            base.Draw(gameTime);
//        }
//    }
//}
