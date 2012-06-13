using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Myre.Entities.Behaviours;
using Myre.Entities;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace Myre.Graphics
{
    [DefaultManager(typeof(Manager))]
    public class View
        : Behaviour
    {
        private Property<Camera> camera;
        private Property<Viewport> viewport;

        public Camera Camera
        {
            get { return camera.Value; }
            set { camera.Value = value; }
        }

        public Viewport Viewport
        {
            get { return viewport.Value; }
            set { viewport.Value = value; }
        }

        public override void CreateProperties(Entity.ConstructionContext context)
        {
            this.camera = context.CreateProperty<Camera>("camera");
            this.viewport = context.CreateProperty<Viewport>("viewport");

            base.CreateProperties(context);
        }

        /*
        public Property<Camera> Camera { get; private set; }
        public Property<Viewport> Viewport { get; private set; }

        public View(
            Property<Camera> camera,
            Property<Viewport> viewport)
        {
            this.Camera = camera;
            this.Viewport = viewport;
        }
        */

        public void SetMetadata(RendererMetadata metadata)
        {
            metadata.Set("activeview", this);
            metadata.Set("resolution", new Vector2(viewport.Value.Width, viewport.Value.Height));
            metadata.Set("viewport", viewport.Value);
            camera.Value.SetMetadata(metadata);
        }


        public class Manager
            : BehaviourManager<View>
        {
            public IEnumerable<View> Views
            {
                get { return Behaviours; }
            }
        }
    }
}
