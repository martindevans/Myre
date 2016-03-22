using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.Xna.Framework.Graphics;
using Ninject;

namespace Myre.Graphics
{
    public class RenderPlan
        : ICloneable, IDisposable
    {
        public struct Output
        {
            private readonly Resource _resource;
            private readonly Renderer _renderer;

            public Texture2D Image
            {
                get { return _resource.RenderTarget; }
            }

            internal Output(Renderer renderer, Resource resource)
            {
                _renderer = renderer;
                _resource = resource;
            }

            public void Finalise()
            {
                _resource.Finalise(_renderer);
            }
        }

        private struct FreePoint
        {
            public string Name;
            public int Index;
        }

        private readonly IKernel _kernel;
        public Renderer Renderer { get; private set; }

        private readonly RendererComponent[] _components;
        private readonly ResourceContext _finalContext;
        private readonly Dictionary<string, int> _resourceLastUsed;
        private readonly Dictionary<string, Resource> _resources;
        private FreePoint[] _freePoints;
        private Resource _output;

        internal RenderPlan(IKernel kernel, Renderer renderer)
        {
            _kernel = kernel;
            Renderer = renderer;
            _components = new RendererComponent[0];
            _resources = new Dictionary<string, Resource>();
            _resourceLastUsed = new Dictionary<string, int>();
            _freePoints = new FreePoint[0];
        }

        private RenderPlan(RenderPlan previous, RendererComponent next)
            :this(previous)
        {
            _components = Append(previous._components, next);

            var context = CreateContext(previous._finalContext);
            next.Initialise(Renderer, context);

            foreach (var resource in context.Inputs)
                _resourceLastUsed[resource] = _components.Length - 1;

            foreach (var resource in context.Outputs)
            {
                if (!_resources.ContainsKey(resource.Name))
                    _resources[resource.Name] = resource;
                _resourceLastUsed[resource.Name] = _components.Length - 1;
            }

            _finalContext = context;

            if (context.Outputs.Count > 0)
                _output = _resources[context.Outputs[0].Name];
            else
            {
                _output = previous._output;
                _resourceLastUsed[_output.Name] = _components.Length - 1;
            }
        }

        private RenderPlan(RenderPlan previous)
        {
            _kernel = previous._kernel;
            Renderer = previous.Renderer;
            _resources = new Dictionary<string, Resource>(previous._resources);
            _resourceLastUsed = new Dictionary<string, int>(previous._resourceLastUsed);
            _components = (RendererComponent[])previous._components.Clone();
            _finalContext = previous._finalContext;
            _output = previous._output;
        }

        ~RenderPlan()
        {
            Dispose(false);
        }

        private void Initialise()
        {
            _freePoints = (from resource in _resourceLastUsed
                           orderby resource.Value ascending
                           select new FreePoint {Name = resource.Key, Index = resource.Value}).ToArray();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private bool _disposed = false;
        private void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
            {
                foreach (var rendererComponent in Components)
                    rendererComponent.Dispose();
            }

            _disposed = true;
        }

        private static RendererComponent[] Append(RendererComponent[] components, RendererComponent next)
        {
            Array.Resize(ref components, components.Length + 1);
            components[components.Length - 1] = next;

            return components;
        }

        private ResourceContext CreateContext(ResourceContext previousContext)
        {
            var available = _resources.Values.Select(r => new ResourceInfo(r.Name, r.Format)).ToArray();
            var setTargets = previousContext == null ? new ResourceInfo[0] : previousContext.Outputs.Where(r => r.IsLeftSet).Select(r => new ResourceInfo(r.Name, r.Format)).ToArray();

            return new ResourceContext(available, setTargets);
        }

        public RenderPlan Then(RendererComponent component)
        {
            return new RenderPlan(this, component);
        }

        public RenderPlan Then<T>()
            where T : RendererComponent
        {
            return Then(_kernel.Get<T>());
        }

        public RenderPlan Show(string resource)
        {
            _resourceLastUsed[resource] = _components.Length - 1;
            _output = _resources[resource];

            return this;
        }

        public void Apply()
        {
            Renderer.Plan = this;
        }

        public Output Execute()
        {
            if (_freePoints == null)
                Initialise();
            Debug.Assert(_freePoints != null, "_freePoints != null");

            int resourceIndex = 0;
            for (int i = 0; i < _components.Length; i++)
            {
                var component = _components[i];
                component.Plan = this;
                component.Draw(Renderer);

                while (resourceIndex < _freePoints.Length && _freePoints[resourceIndex].Index <= i)
                {
                    var point = _freePoints[resourceIndex];
                    if (point.Name != _output.Name)
                        _resources[point.Name].Finalise(Renderer);

                    resourceIndex++;
                }
            }

            return new Output(Renderer, _output);
        }

        internal RenderTarget2D GetResource(string name)
        {
            return _resources[name].RenderTarget;
        }

        internal void SetResource(string name, RenderTarget2D resource)
        {
            _resources[name].RenderTarget = resource;
            Renderer.Data.Set(new TypedName<Texture2D>(name), resource);
        }

        public IEnumerable<string> Resources
        {
            get { return _resources.Keys; }
        }

        public IEnumerable<RendererComponent> Components
        {
            get { return _components; }
        }

        public RenderPlan Clone()
        {
            return new RenderPlan(this);
        }

        object ICloneable.Clone()
        {
            return Clone();
        }
    }
}
