using System;
using System.Collections.Generic;
using System.Numerics;
using Microsoft.Xna.Framework.Graphics;
using Myre.Entities;
using Myre.Entities.Behaviours;

namespace Myre.Graphics
{
    [DefaultManager(typeof(Manager))]
    public class View
        : ProcessBehaviour
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
            _camera = context.CreateProperty(new TypedName<Camera>("camera"));
            _viewport = context.CreateProperty(new TypedName<Viewport>("viewport"));

            base.CreateProperties(context);
        }

        public virtual void SetMetadata(RendererMetadata metadata)
        {
            metadata.Set("activeview", this);
            metadata.Set("resolution", new Vector2(_viewport.Value.Width, _viewport.Value.Height));
            metadata.Set("viewport", _viewport.Value);
            metadata.Set("aspectratio", _viewport.Value.AspectRatio);
            _camera.Value.SetMetadata(metadata);
        }

        private Renderer _currentRenderer;

        /// <summary>
        /// Indicates that the given renderer is about to render this view
        /// </summary>
        /// <param name="renderer"></param>
        public virtual void Begin(Renderer renderer)
        {
            if (_currentRenderer != null)
                throw new InvalidOperationException("Cannot 'Begin' rendering a view whilst it is already begun");
            _currentRenderer = renderer;
        }

        /// <summary>
        /// Indicates that the given renderer has finished rendering this view
        /// </summary>
        /// <param name="renderer"></param>
        public virtual void End(Renderer renderer)
        {
            if (_currentRenderer != renderer)
                throw new InvalidOperationException("Cannot 'End' rendering a view whilst begun with a different view");
            _currentRenderer = null;
        }

        protected override void Update(float elapsedTime)
        {
        }

        public class Manager
            : Manager<View>
        {
            public IEnumerable<View> Views
            {
                get { return Behaviours; }
            }
        }
    }
}
