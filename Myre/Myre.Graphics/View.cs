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
        private Property<Camera> _camera;
        private Property<Viewport> _viewport;

        public Camera Camera
        {
            get { return _camera.Value; }
            set { _camera.Value = value; }
        }

        public Viewport Viewport
        {
            get { return _viewport.Value; }
            set { _viewport.Value = value; }
        }

        public override void CreateProperties(Entity.ConstructionContext context)
        {
            _camera = context.CreateProperty<Camera>("camera");
            _viewport = context.CreateProperty<Viewport>("viewport");

            base.CreateProperties(context);
        }

        public void SetMetadata(RendererMetadata metadata)
        {
            metadata.Set("activeview", this);
            metadata.Set("resolution", new Vector2(_viewport.Value.Width, _viewport.Value.Height));
            metadata.Set("viewport", _viewport.Value);
            _camera.Value.SetMetadata(metadata);
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
