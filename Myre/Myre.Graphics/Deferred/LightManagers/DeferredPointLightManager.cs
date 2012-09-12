using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Myre.Entities.Behaviours;
using Myre.Graphics.Lighting;
using Myre.Graphics.Materials;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework;

namespace Myre.Graphics.Deferred.LightManagers
{
    public class DeferredPointLightManager
            : BehaviourManager<PointLight>, IDirectLight
    {
        private Material geometryLightingMaterial;
        private Material quadLightingMaterial;
        private Quad quad;
        private Model geometry;

        private List<PointLight> touchesNearPlane;
        private List<PointLight> touchesBothPlanes;
        private List<PointLight> doesntTouchNear;

        private DepthStencilState depthGreater;
        private DepthStencilState depthLess;
        private BlendState colourWriteDisable;
        private DepthStencilState stencilWritePass;
        private DepthStencilState stencilCheckPass;

        public DeferredPointLightManager(GraphicsDevice device)
        {
            var effect = Content.Load<Effect>("PointLight");
            geometryLightingMaterial = new Material(effect.Clone(), "Geometry");
            quadLightingMaterial = new Material(effect.Clone(), "Quad");
            
            geometry = Content.Load<Model>("sphere");

            quad = new Quad(device);

            touchesNearPlane = new List<PointLight>();
            touchesBothPlanes = new List<PointLight>();
            doesntTouchNear = new List<PointLight>();

            depthGreater = new DepthStencilState()
            {
                DepthBufferEnable = true,
                DepthBufferWriteEnable = false,
                DepthBufferFunction = CompareFunction.GreaterEqual
            };

            depthLess = new DepthStencilState()
            {
                DepthBufferEnable = true,
                DepthBufferWriteEnable = false,
                DepthBufferFunction = CompareFunction.LessEqual
            };
        }

        public override void Add(PointLight behaviour)
        {
            base.Add(behaviour);
        }

        public void Prepare(Renderer renderer)
        {
            touchesNearPlane.Clear();
            touchesBothPlanes.Clear();
            doesntTouchNear.Clear();

            var frustum = renderer.Data.Get<BoundingFrustum>("viewfrustum").Value;
            
            foreach (var light in Behaviours)
            {
                var bounds = new BoundingSphere(light.Position, light.Range);
                if (!bounds.Intersects(frustum))
                    continue;

                var near = bounds.Intersects(frustum.Near) == PlaneIntersectionType.Intersecting;
                var far = bounds.Intersects(frustum.Far) == PlaneIntersectionType.Intersecting;

                if (near && far)
                    touchesBothPlanes.Add(light);
                else if (near)
                    touchesNearPlane.Add(light);
                else
                    doesntTouchNear.Add(light);
            }
        }


        public void Draw(Renderer renderer)
        {
            var metadata = renderer.Data;
            var device = renderer.Device;

            // set deice for drawing sphere mesh
            var part = geometry.Meshes[0].MeshParts[0];
            device.SetVertexBuffer(part.VertexBuffer);
            device.Indices = part.IndexBuffer;

            // draw lights which touch near plane
            // back faces, cull those in front of geometry
            device.DepthStencilState = depthGreater;
            device.RasterizerState = RasterizerState.CullClockwise;
            DrawGeometryLights(touchesNearPlane, metadata, device);

            // draw lights which touch both planes
            // full screen quad
            device.DepthStencilState = DepthStencilState.None;
            device.RasterizerState = RasterizerState.CullCounterClockwise;
            foreach (var light in touchesBothPlanes)
            {
                SetupLight(metadata, quadLightingMaterial, light);
                quad.Draw(quadLightingMaterial, metadata);
            }

            // draw all other lights
            // front faces, cull those behind geometry
            device.DepthStencilState = depthLess;
            DrawGeometryLights(doesntTouchNear, metadata, device);
        }

        private void DrawGeometryLights(List<PointLight> lights, RendererMetadata metadata, GraphicsDevice device)
        {
            foreach (var light in lights)
            {
                SetupLight(metadata, geometryLightingMaterial, light);
                DrawGeomery(geometryLightingMaterial, metadata, device);
            }
        }

        private void DrawGeomery(Material material, RendererMetadata metadata, GraphicsDevice device)
        {
            var part = geometry.Meshes[0].MeshParts[0];
            foreach (var pass in material.Begin(metadata))
            {
                pass.Apply();

                device.DrawIndexedPrimitives(PrimitiveType.TriangleList,
                    part.VertexOffset, 0, part.NumVertices, part.StartIndex, part.PrimitiveCount);
            }
        }

        private void SetupLight(RendererMetadata metadata, Material material, PointLight light)
        {
            if (material != null)
            {
                Vector3 position = Vector3.Transform(light.Position, metadata.Get<Matrix>("view").Value);
                material.Parameters["LightPosition"].SetValue(position);
                material.Parameters["Colour"].SetValue(light.Colour);
                material.Parameters["Range"].SetValue(light.Range);
            }

            var world = Matrix.CreateScale(light.Range / geometry.Meshes[0].BoundingSphere.Radius)
                        * Matrix.CreateTranslation(light.Position);
            metadata.Set<Matrix>("world", world);
            Matrix.Multiply(ref world, ref metadata.Get<Matrix>("view").Value, out metadata.Get<Matrix>("worldview").Value);
            Matrix.Multiply(ref metadata.Get<Matrix>("worldview").Value, ref metadata.Get<Matrix>("projection").Value, out metadata.Get<Matrix>("worldviewprojection").Value);
        }
    }
}
