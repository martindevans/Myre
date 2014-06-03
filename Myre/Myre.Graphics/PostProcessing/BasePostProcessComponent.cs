using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Myre.Graphics.Materials;

namespace Myre.Graphics.PostProcessing
{
    public abstract class BasePostProcessComponent
        :RendererComponent
    {
        private readonly Material _effect;
        private readonly SurfaceFormat _format;
        private readonly Quad _quad;

        private InputBinding[] _inputs;
        private string _output;

        protected BasePostProcessComponent(GraphicsDevice device, Effect effect, string technique, SurfaceFormat format = SurfaceFormat.Color)
        {
            _effect = new Material(effect, technique);
            _format = format;
            _quad = new Quad(device);
        }

        protected new abstract IEnumerable<InputBinding> Inputs(ResourceContext context);

        protected abstract string Output();

        public override void Initialise(Renderer renderer, ResourceContext context)
        {
            _inputs = Inputs(context).ToArray();
            foreach (var input in _inputs)
                context.DefineInput(input.ResourceName);

            _output = Output();
            context.DefineOutput(_output, true);

            base.Initialise(renderer, context);
        }

        protected abstract void SetEffectParameters(Material e, RendererMetadata metadata);

        public override void Draw(Renderer renderer)
        {
            var metadata = renderer.Data;
            var device = renderer.Device;

            var resolution = metadata.GetValue(new TypedName<Vector2>("resolution"));
            var width = (int)resolution.X;
            var height = (int)resolution.Y;

            var outputTarget = RenderTargetManager.GetTarget(device, width, height, _format, DepthFormat.None, name: _output);
            device.SetRenderTarget(outputTarget);

            device.Clear(Color.Black);
            SetEffectParameters(_effect, metadata);
            for (int i = 0; i < _inputs.Length; i++) //Bind input resources
            {
                var b = _inputs[i];
                _effect.Parameters[b.ShaderParameter].SetValue(GetResource(b.ResourceName));
            }
            _quad.Draw(_effect, metadata);

            Output(_output, outputTarget);
        }

        protected struct InputBinding
        {
            public readonly string ShaderParameter;
            public readonly string ResourceName;

            public InputBinding(string parameter, string name)
            {
                ShaderParameter = parameter;
                ResourceName = name;
            }
        }
    }
}
