using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Myre.Debugging.Statistics;
using Myre.Entities;
using Myre.Entities.Services;
using Myre.Extensions;
using Ninject;
using System;

namespace Myre.Graphics
{
    public class Renderer
        : Service
    {
        struct Resource
        {
            public string Name;
            public RenderTarget2D Target;
        }

        private RendererMetadata data;
        private RendererSettings settings;
        private Queue<RenderPlan.Output> viewResults;

        private IKernel kernel;
        private GraphicsDevice device;
        private Quad quad;
        private SpriteBatch spriteBatch;

        private RenderPlan plan;

        private Scene scene;
        private IEnumerable<View> views;

        public GraphicsDevice Device
        {
            get { return device; }
        }

        public Scene Scene
        {
            get { return scene; }
        }

        public RendererMetadata Data
        {
            get { return data; }
        }

        public RendererSettings Settings
        {
            get { return settings; }
        }

        public RenderPlan Plan
        {
            get { return plan; }
            set { plan = value; }
        }

        public Renderer(IKernel kernel, GraphicsDevice device, IServiceProvider services)
        {
            this.kernel = kernel;
            this.device = device;
            this.data = new RendererMetadata();
            this.settings = new RendererSettings(this);
            this.viewResults = new Queue<RenderPlan.Output>();
            this.quad = new Quad(device);
            this.spriteBatch = new SpriteBatch(device);

            Content.Initialise(services);
        }

        public override void Initialise(Scene scene)
        {
            this.scene = scene;

            this.views = from manager in scene.FindManagers<View.Manager>()
                         from view in manager.Views
                         select view;

            base.Initialise(scene);
        }

        public override void Update(float elapsedTime)
        {
            data.Set<float>("timedelta", elapsedTime);
            base.Update(elapsedTime);
        }

        public override void Draw()
        {
#if WINDOWS
            var targets = device.GetRenderTargets();
#endif

#if PROFILE
            Statistic.Get("Graphics.Primitives").Value = 0;
            Statistic.Get("Graphics.Draws").Value = 0;
#endif

            foreach (var view in views)
            {
                view.SetMetadata(data);
                var output = Plan.Execute(this);

                viewResults.Enqueue(output);
            }

#if WINDOWS
            if (targets.Length != 0)
                device.SetRenderTargets(targets);
            else
                device.SetRenderTarget(null);
#else
            device.SetRenderTarget(null);
#endif


            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend);
            foreach (var view in views)
            {
                var output = viewResults.Dequeue();
                var viewport = view.Viewport;

                if (output.Image.Format.IsFloatingPoint())
                    device.SamplerStates[0] = SamplerState.PointClamp;
                else
                    device.SamplerStates[0] = SamplerState.LinearClamp;

                spriteBatch.Draw(output.Image, viewport.Bounds, Color.White);
                output.Finalise();
            }
            spriteBatch.End();

            base.Draw();
        }

        public RenderPlan StartPlan()
        {
            return new RenderPlan(kernel, this);
        }
    }
}
