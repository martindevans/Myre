using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Myre.Entities.Behaviours;
using Myre.Graphics.Lighting;
using Myre.Graphics.Materials;

namespace Myre.Graphics.Deferred.LightManagers
{
    public class DeferredPlaneLightManager
        : BehaviourManager<PlaneLight>, IDirectLight
    {
        private readonly Material _geometryLightingMaterial;
        private readonly Material _quadLightingMaterial;
        private readonly Quad _quad;
        private readonly Model _geometry;

        private readonly List<PlaneLight> _touchesNearPlane = new List<PlaneLight>();
        private readonly List<PlaneLight> _touchesBothPlanes = new List<PlaneLight>();
        private readonly List<PlaneLight> _doesntTouchNear = new List<PlaneLight>();

        private readonly DepthStencilState _depthGreater;
        private readonly DepthStencilState _depthLess;

        public DeferredPlaneLightManager(GraphicsDevice device)
        {
            //var effect = Content.Load<Effect>("PlaneLight");
            //_geometryLightingMaterial = new Material(effect.Clone(), "Geometry");
            //_quadLightingMaterial = new Material(effect.Clone(), "Quad");
            
            //_geometry = Content.Load<Model>("sphere");

            //_quad = new Quad(device);

            //_depthGreater = new DepthStencilState()
            //{
            //    DepthBufferEnable = true,
            //    DepthBufferWriteEnable = false,
            //    DepthBufferFunction = CompareFunction.GreaterEqual
            //};

            //_depthLess = new DepthStencilState()
            //{
            //    DepthBufferEnable = true,
            //    DepthBufferWriteEnable = false,
            //    DepthBufferFunction = CompareFunction.LessEqual
            //};
        }

        public void Prepare(Renderer renderer)
        {
            //_touchesNearPlane.Clear();
            //_touchesBothPlanes.Clear();
            //_doesntTouchNear.Clear();

            //var frustum = renderer.Data.Get<BoundingFrustum>("viewfrustum").Value;
            
            //foreach (var light in Behaviours)
            //{
            //    var bounds = new BoundingSphere(light.Position, light.Range);
            //    if (!bounds.Intersects(frustum))
            //        continue;

            //    var near = bounds.Intersects(frustum.Near) == PlaneIntersectionType.Intersecting;
            //    var far = bounds.Intersects(frustum.Far) == PlaneIntersectionType.Intersecting;

            //    if (near && far)
            //        _touchesBothPlanes.Add(light);
            //    else if (near)
            //        _touchesNearPlane.Add(light);
            //    else
            //        _doesntTouchNear.Add(light);
            //}
        }

        public void Draw(Renderer renderer)
        {
            //var metadata = renderer.Data;
            //var device = renderer.Device;

            //// set deice for drawing sphere mesh
            //var part = _geometry.Meshes[0].MeshParts[0];
            //device.SetVertexBuffer(part.VertexBuffer);
            //device.Indices = part.IndexBuffer;

            //// draw lights which touch near plane
            //// back faces, cull those in front of geometry
            //device.DepthStencilState = _depthGreater;
            //device.RasterizerState = RasterizerState.CullClockwise;
            //DrawGeometryLights(_touchesNearPlane, metadata, device);

            //// draw lights which touch both planes
            //// full screen quad
            //device.DepthStencilState = DepthStencilState.None;
            //device.RasterizerState = RasterizerState.CullCounterClockwise;
            //foreach (var light in _touchesBothPlanes)
            //{
            //    SetupLight(metadata, _quadLightingMaterial, light);
            //    _quad.Draw(_quadLightingMaterial, metadata);
            //}

            //// draw all other lights
            //// front faces, cull those behind geometry
            //device.DepthStencilState = _depthLess;
            //DrawGeometryLights(_doesntTouchNear, metadata, device);
        }

        // ReSharper disable ParameterTypeCanBeEnumerable.Local
        private void DrawGeometryLights(List<PlaneLight> lights, RendererMetadata metadata, GraphicsDevice device)
        // ReSharper restore ParameterTypeCanBeEnumerable.Local
        {
            foreach (var light in lights)
            {
                SetupLight(metadata, _geometryLightingMaterial, light);
                DrawGeomery(_geometryLightingMaterial, metadata, device);
            }
        }

        private void DrawGeomery(Material material, RendererMetadata metadata, GraphicsDevice device)
        {
            var part = _geometry.Meshes[0].MeshParts[0];
            foreach (var pass in material.Begin(metadata))
            {
                pass.Apply();

                device.DrawIndexedPrimitives(PrimitiveType.TriangleList,
                    part.VertexOffset, 0, part.NumVertices, part.StartIndex, part.PrimitiveCount);
            }
        }

        private void SetupLight(RendererMetadata metadata, Material material, PlaneLight light)
        {
            var view = metadata.Get<Matrix>("view");
            var worldview = metadata.Get<Matrix>("worldview");
            var worldviewprojection = metadata.Get<Matrix>("worldviewprojection");
            var projection = metadata.Get<Matrix>("projection");

            var world = Matrix.CreateScale(light.Range / _geometry.Meshes[0].BoundingSphere.Radius) * Matrix.CreateTranslation(light.Position);
            metadata.Set<Matrix>("world", world);
            Matrix.Multiply(ref world, ref view.Value, out worldview.Value);
            Matrix.Multiply(ref worldview.Value, ref projection.Value, out worldviewprojection.Value);

            if (material != null)
            {
                Vector3 position = Vector3.Transform(light.Position, metadata.Get<Matrix>("view").Value);
                material.Parameters["LightPosition"].SetValue(position);
                material.Parameters["Texture"].SetValue(light.Texture);
                material.Parameters["Range"].SetValue(light.Range);
                material.Parameters["PlaneMatrix"].SetValue(light.PlaneMatrix);
                material.Parameters["inverseView"].SetValue(Matrix.Invert(view.Value));
            }
        }
    }
}
