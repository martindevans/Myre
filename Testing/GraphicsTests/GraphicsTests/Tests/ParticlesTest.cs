using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Myre.Entities;
using Myre.Graphics.Particles;
using Ninject;
using Myre.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace GraphicsTests.Tests
{
    class ParticlesTest
        : TestScreen
    {
        private IKernel kernel;
        private Scene scene;
        private ContentManager content;
        private GraphicsDevice device;
        private EllipsoidParticleEmitter emitter;
        private Camera camera;

        public ParticlesTest(IKernel kernel, ContentManager content, GraphicsDevice device)
            : base("Particles", kernel)
        {
            this.kernel = kernel;
            this.content = content;
            this.device = device;
        }

        protected override void BeginTransitionOn()
        {
            if (scene != null)
                return;

            scene = kernel.Get<Scene>();

            var renderer = scene.GetService<Renderer>();
            renderer.StartPlan()
                .Then(new CreateTargetComponent(new RenderTargetInfo() { DepthFormat = DepthFormat.Depth24Stencil8 }))
                .Then<ParticleComponent>()
                .Apply();

            var cameraPosition = new Vector3(0, 25, -200);

            camera = new Camera();
            camera.NearClip = 1;
            camera.FarClip = 3000;
            camera.View = Matrix.CreateLookAt(cameraPosition, new Vector3(0, 25, 0), Vector3.Up);
            camera.Projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(60), 16f / 9f, camera.NearClip, camera.FarClip);

            var cameraDescription = kernel.Get<EntityDescription>();
            cameraDescription.AddProperty<Viewport>("viewport");
            cameraDescription.AddBehaviour<View>();
            var cameraEntity = cameraDescription.Create();
            cameraEntity.GetProperty<Camera>("camera").Value = camera;
            cameraEntity.GetProperty<Viewport>("viewport").Value = new Viewport() { Width = device.PresentationParameters.BackBufferWidth, Height = device.PresentationParameters.BackBufferHeight };
            scene.Add(cameraEntity);

            var particleEntityDesc = kernel.Get<EntityDescription>();
            particleEntityDesc.AddProperty<Vector3>("position");
            particleEntityDesc.AddBehaviour<EllipsoidParticleEmitter>();
            var entity = particleEntityDesc.Create();
            emitter = entity.GetBehaviour<EllipsoidParticleEmitter>();
            entity.GetProperty<Vector3>("position").Value = Vector3.Zero;
            scene.Add(entity);

            var white = new Texture2D(device, 1, 1);
            white.SetData(new Color[] { Color.White });

            emitter.BlendState = BlendState.Additive;
            emitter.Enabled = true;
            emitter.EndLinearVelocity = 0.25f;
            emitter.EndScale = 0.75f;
            emitter.Gravity = Vector3.Zero;//new Vector3(0, 15, 0);
            emitter.Lifetime = 4f;
            emitter.Texture = content.Load<Texture2D>("fire");
            emitter.EmitPerSecond = 500;
            emitter.Capacity = (int)(emitter.Lifetime * emitter.EmitPerSecond + 1);
            emitter.HorizontalVelocityVariance = 10;// 20;
            emitter.LifetimeVariance = 0f;
            emitter.MaxAngularVelocity = MathHelper.Pi / 4;
            emitter.MaxEndColour = Color.Blue; //Color.White;
            emitter.MaxStartColour = Color.White;
            emitter.MaxStartSize = 40;
            emitter.MinAngularVelocity = -MathHelper.Pi / 4;
            emitter.MinEndColour = Color.White;
            emitter.MinStartColour = Color.Red;
            emitter.MinStartSize = 30;
            emitter.Transform = Matrix.Identity;
            emitter.Velocity = Vector3.Zero;//new Vector3(0, 1, 0);
            emitter.VelocityBleedThrough = 0;
            emitter.VerticalVelocityVariance = 10;// 20f;
            emitter.Ellipsoid = new Vector3(10, 100, 100);
            emitter.MinEmitDistance = 75;
            
            base.OnShown();
        }

        public override void Update(Microsoft.Xna.Framework.GameTime gameTime)
        {
            camera.View = Matrix.CreateLookAt(new Vector3((float)Math.Sin(gameTime.TotalGameTime.TotalSeconds) * 300, 0, (float)Math.Cos(gameTime.TotalGameTime.TotalSeconds) * 300), new Vector3(0, 0, 0), Vector3.Up);
            scene.Update((float)gameTime.ElapsedGameTime.TotalSeconds);
            base.Update(gameTime);
        }

        public override void Draw(Microsoft.Xna.Framework.GameTime gameTime)
        {
            scene.Draw();
            base.Draw(gameTime);
        }
    }
}
