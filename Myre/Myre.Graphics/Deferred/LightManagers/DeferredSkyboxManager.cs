using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Myre.Entities.Behaviours;
using Myre.Graphics.Lighting;

namespace Myre.Graphics.Deferred.LightManagers
{
    public class DeferredSkyboxManager
            : BehaviourManager<Skybox>, IDirectLight
    {
        readonly Model _model;
        readonly Effect _skyboxEffect;
        readonly Quad _quad;

        private readonly DepthStencilState _depthState = new DepthStencilState
        {
            DepthBufferEnable = true,
            DepthBufferWriteEnable = false,
            DepthBufferFunction = CompareFunction.LessEqual
        };

        public DeferredSkyboxManager(GraphicsDevice device)
        {
            _skyboxEffect = Content.Load<Effect>("Skybox");
            _model = Content.Load<Model>("SkyboxModel");
            _quad = new Quad(device);
            _quad.SetPosition(depth: 1);
        }

        public void Prepare(Renderer renderer)
        {
        }

        public void Draw(Renderer renderer)
        {
            var device = renderer.Device;

            var previousDepthState = device.DepthStencilState;
            device.DepthStencilState = _depthState;

            var previousRasterState = device.RasterizerState;
            device.RasterizerState = RasterizerState.CullNone;

            var part = _model.Meshes[0].MeshParts[0];
            device.SetVertexBuffer(part.VertexBuffer);
            device.Indices = part.IndexBuffer;

            var view = renderer.Data.GetValue(new TypedName<Matrix>("view"));
            view.Translation = Vector3.Zero;
            _skyboxEffect.Parameters["View"].SetValue(view);
            _skyboxEffect.Parameters["Projection"].SetValue(renderer.Data.GetValue(new TypedName<Matrix>("projection")));

            for (int i = 0; i < Behaviours.Count; i++)
            {
                var light = Behaviours[i];
                _skyboxEffect.Parameters["EnvironmentMap"].SetValue(light.Texture);
                _skyboxEffect.Parameters["Brightness"].SetValue(light.Brightness);

                _skyboxEffect.CurrentTechnique = light.GammaCorrect ? _skyboxEffect.Techniques["SkyboxGammaCorrect"] : _skyboxEffect.Techniques["Skybox"];

                foreach (var pass in _skyboxEffect.CurrentTechnique.Passes)
                {
                    pass.Apply();
                    //device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, 8, 0, 12);
                    device.DrawIndexedPrimitives(PrimitiveType.TriangleList, part.VertexOffset, 0, part.NumVertices, part.StartIndex, part.PrimitiveCount);
                }
            }

            device.DepthStencilState = previousDepthState;
            device.RasterizerState = previousRasterState;
        }
    }
}
