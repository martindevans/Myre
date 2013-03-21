using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework.Graphics;

namespace Myre.Graphics
{
    class Resource
    {
        public static readonly Action<Renderer, RenderTarget2D> DefaultFinaliser = (renderer, target) => RenderTargetManager.RecycleTarget(target);

        public string Name;
        public RenderTargetInfo Format;
        public Action<Renderer, RenderTarget2D> Finaliser;
        public bool IsLeftSet;
        public RenderTarget2D RenderTarget;

        public void Finalise(Renderer renderer)
        {
            Finaliser(renderer, RenderTarget);
        }

        public void Finalise(Renderer renderer, RenderTarget2D target)
        {
            Finaliser(renderer, target);
        }
    }

    /// <summary>
    /// Represents information about a resource produced by a renderer component.
    /// </summary>
    public struct ResourceInfo
    {
        /// <summary>
        /// Gets or sets the name of the resource.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the format of the resource.
        /// </summary>
        public RenderTargetInfo Format { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ResourceInfo"/> struct.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="format">The format.</param>
        public ResourceInfo(string name, RenderTargetInfo format)
            : this()
        {
            Name = name;
            Format = format;
        }
    }

    /// <summary>
    /// Provides mechanisms for renderer components to validate their input
    /// resources and define their output resources.
    /// </summary>
    public class ResourceContext
    {
        private readonly List<string> _inputs;
        private readonly List<Resource> _outputs;

        /// <summary>
        /// All resources available to the component.
        /// </summary>
        public ResourceInfo[] AvailableResources { get; private set; }

        /// <summary>
        /// The render targets set on the device prior to this components' turn to draw.
        /// </summary>
        public ResourceInfo[] SetRenderTargets { get; private set; }

// ReSharper disable ReturnTypeCanBeEnumerable.Global
        internal List<string> Inputs { get { return _inputs; } }
// ReSharper restore ReturnTypeCanBeEnumerable.Global
        internal List<Resource> Outputs { get { return _outputs; } }

        public ResourceContext(ResourceInfo[] availableResources, ResourceInfo[] setRenderTargets)
        {
            AvailableResources = availableResources;
            SetRenderTargets = setRenderTargets;
            _inputs = new List<string>();
            _outputs = new List<Resource>();
        }

        /// <summary>
        /// Informs the render plan that this component wants the specified resource as input.
        /// </summary>
        /// <param name="name">The name of the resource.</param>
        public void DefineInput(string name)
        {
            if (!AvailableResources.Any(info => info.Name == name))
                throw new ArgumentException(string.Format("The resource {0} is not available.", name));

            _inputs.Add(name);
        }

        /// <summary>
        /// Informs the renderer of a resource this component is generating.
        /// </summary>
        /// <param name="name">The name of the resource.</param>
        /// <param name="isLeftSet"><c>true</c> if this component will leave the render target set on the device; else <c>false</c>.</param>
        /// <param name="finaliser">A cleanup method, called when the resource is no longer needed. <c>null</c> for the default finaliser.</param>
        /// <param name="width">The width of the render target.</param>
        /// <param name="height">The height of the render target.</param>
        /// <param name="surfaceFormat">The surface format of the render target.</param>
        /// <param name="depthFormat">The depth format of the render target.</param>
        /// <param name="multiSampleCount">The multi sample count of the render target.</param>
        /// <param name="mipMap">The number of mip map levels of the render target.</param>
        /// <param name="usage">The render target usage of the render target.</param>
        public void DefineOutput(
            string name,
            bool isLeftSet = true,
            Action<Renderer, RenderTarget2D> finaliser = null,
            int width = 0,
            int height = 0,
            SurfaceFormat surfaceFormat = SurfaceFormat.Color,
            DepthFormat depthFormat = DepthFormat.None,
            int multiSampleCount = 0,
            bool mipMap = false,
            RenderTargetUsage usage = RenderTargetUsage.DiscardContents)
        {
            var info = new RenderTargetInfo(width, height, surfaceFormat, depthFormat, multiSampleCount, mipMap, usage);

            DefineOutput(name, isLeftSet, finaliser, info);
        }

        /// <summary>
        /// Informs the renderer of a resource this component is generating.
        /// </summary>
        /// <param name="name">The name of the resource.</param>
        /// <param name="isLeftSet"><c>true</c> if this component will leave the render target set on the device; else <c>false</c>.</param>
        /// <param name="finaliser">A cleanup method, called when the resource is no longer needed. <c>null</c> for the default finaliser.</param>
        /// <param name="format">The format of the render target.</param>
        public void DefineOutput(
            string name,
            bool isLeftSet,
            Action<Renderer, RenderTarget2D> finaliser,
            RenderTargetInfo format)
        {
            var resource = new Resource()
            {
                Name = name,
                Finaliser = finaliser ?? Resource.DefaultFinaliser,
                IsLeftSet = isLeftSet,
                Format = format
            };

            _outputs.Add(resource);
        }

        public void DefineOutput(ResourceInfo resourceInfo, bool isLeftSet = true)
        {
            var resource = new Resource()
            {
                Name = resourceInfo.Name,
                Finaliser = Resource.DefaultFinaliser,
                IsLeftSet = isLeftSet,
                Format = resourceInfo.Format
            };

            _outputs.Add(resource);
        }
    }

    /// <summary>
    /// An object which represents a stage of a render plan.
    /// Each renderer component encapsulates drawing onto a render target,
    /// while providing its' output a a resource for other renderer components to consume.
    /// </summary>
    public abstract class RendererComponent
    {
        internal RenderPlan Plan;

        /// <summary>
        /// Gets or sets a value indicating if this component has been initialised.
        /// </summary>
        public bool Initialised { get; private set; }

        /// <summary>
        /// Initialised this renderer component.
        /// </summary>
        /// <remarks>
        /// Here, components may validate that required inputs are available, and
        /// define their output.
        /// </remarks>
        /// <param name="renderer">The renderer.</param>
        /// <param name="context">The resource context containing environment information and providing means to define inputs and outputs.</param>
        public virtual void Initialise(Renderer renderer, ResourceContext context)
        {
            Initialised = true;
        }

        /// <summary>
        /// Draws this component.
        /// </summary>
        /// <param name="renderer">The renderer</param>
        public abstract void Draw(Renderer renderer);

        /// <summary>
        /// Gets a named resource.
        /// </summary>
        /// <param name="name">The name of the resource.</param>
        /// <returns>A resource outputted from a previous component.</returns>
        protected RenderTarget2D GetResource(string name)
        {
            return Plan.GetResource(name);
        }

        /// <summary>
        /// Outputs a resource.
        /// </summary>
        /// <param name="name">The name of the resource.</param>
        /// <param name="resource">The resource.</param>
        protected void Output(string name, RenderTarget2D resource)
        {
            Plan.SetResource(name, resource);
        }

        /*
        public class Resource
        {
            public string Name;
            public Action<Renderer, Resource> Finaliser;
            public bool IsLeftSet;

            public Resource()
            {
                Name = null;
                Finaliser = (r, o) => r.FreeResource(o.Name);
            }

            internal void Finalise(Renderer renderer)
            {
                if (Finaliser != null)
                    Finaliser(renderer, this);
            }
        }

        public class Input
        {
            public string Name;
            public bool Optional;
        }

        public ReadOnlyCollection<Input> InputResources { get; private set; }
        public ReadOnlyCollection<Resource> OutputResources { get; private set; }
        public RenderTargetInfo? OutputTarget { get; private set; }
        public bool Initialised { get; private set; }
        
        protected abstract void SpecifyResources(IList<Input> inputResources, IList<Resource> outputResources, out RenderTargetInfo? outputTarget);
        protected internal abstract bool ValidateInput(RenderTargetInfo? previousRenderTarget);

        internal void SpecifyResources()
        {
            var inputs = new List<Input>();
            var outputs = new List<Resource>();
            RenderTargetInfo? outputTarget;
            SpecifyResources(inputs, outputs, out outputTarget);

            this.InputResources = new ReadOnlyCollection<Input>(inputs);
            this.OutputResources = new ReadOnlyCollection<Resource>(outputs);
            this.OutputTarget = outputTarget;
        }

        public abstract RenderTarget2D Draw(Renderer renderer);
        public virtual void Initialise(Renderer renderer) { Initialised = true; }
         */
    }
}
