using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Myre.Entities;
using Myre.Entities.Behaviours;

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
            camera = context.CreateProperty<Camera>("camera");
            viewport = context.CreateProperty<Viewport>("viewport");

            base.CreateProperties(context);
        }

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
