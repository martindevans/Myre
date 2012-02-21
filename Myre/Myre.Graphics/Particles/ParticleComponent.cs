using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace Myre.Graphics.Particles
{
    public class ParticleComponent
        : RendererComponent
    {
        private ReadOnlyCollection<ParticleEmitter.Manager> managers;

        public override void Initialise(Renderer renderer, ResourceContext context)
        {
            managers = renderer.Scene.FindManagers<ParticleEmitter.Manager>();

            // define inputs
            if (context.AvailableResources.Any(r => r.Name == "gbuffer_depth"))
                context.DefineInput("gbuffer_depth");

            // define outputs
            foreach (var resource in context.SetRenderTargets)
                context.DefineOutput(resource);

            base.Initialise(renderer, context);
        }

        public override void Draw(Renderer renderer)
        {
            foreach (var item in managers)
                item.Draw(renderer);
        }
    }
}
