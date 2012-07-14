// todo: replace sphere geometry approximation with cone

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Myre.Entities.Behaviours;
using Myre.Graphics.Lighting;
using Myre.Graphics.Materials;
using Microsoft.Xna.Framework.Graphics;
using Ninject;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework;
using Myre.Entities;
using Myre.Graphics.Geometry;

namespace Myre.Graphics.Deferred.LightManagers
{
    public class DeferredSpotLightManager
            : BehaviourManager<SpotLight>, IDirectLight
    {
        class LightData
        {
            public SpotLight Light;
            public RenderTarget2D ShadowMap;
            public Matrix View;
            public Matrix Projection;
        }

        private Material geometryLightingMaterial;
        private Material quadLightingMaterial;
        private Material nothingMaterial;
        private Quad quad;
        private Model geometry;
        private View shadowView;

        //private BasicEffect basicEffect;
        //private VertexPositionColor[] debugVertices;
        //private int[] debugIndices;

        private List<LightData> lights;
        private List<LightData> touchesNearPlane;
        private List<LightData> touchesFarPlane;
        private List<LightData> touchesBothPlanes;
        private List<LightData> touchesNeitherPlane;

        private DepthStencilState depthGreater;
        private BlendState colourWriteDisable;
        private DepthStencilState stencilWritePass;
        private DepthStencilState stencilCheckPass;
        
        public DeferredSpotLightManager(IKernel kernel, GraphicsDevice device)
        {
            var effect = Content.Load<Effect>("SpotLight");
            geometryLightingMaterial = new Material(effect.Clone(), "Geometry");
            quadLightingMaterial = new Material(effect.Clone(), "Quad");

            effect = Content.Load<Effect>("Nothing");
            nothingMaterial = new Material(effect, null);

            //basicEffect = new BasicEffect(device);
            //debugVertices = new VertexPositionColor[10];
            //debugIndices = new int[(debugVertices.Length - 1) * 2 * 2];
            //for (int i = 1; i < debugVertices.Length; i++)
            //{
            //    debugVertices[i] = new VertexPositionColor(
            //        new Vector3(
            //            (float)Math.Sin(i * (MathHelper.TwoPi / (debugVertices.Length - 1))),
            //            (float)Math.Cos(i * (MathHelper.TwoPi / (debugVertices.Length - 1))),
            //            -1),
            //        Color.White);

            //    var index = (i - 1) * 4;
            //    debugIndices[index] = 0;
            //    debugIndices[index + 1] = i;
            //    debugIndices[index + 2] = i;
            //    debugIndices[index + 3] = (i % (debugVertices.Length - 1)) + 1; //i < debugVertices.Length - 1 ? i + 1 : 1;
            //}

            geometry = Content.Load<Model>("sphere");

            quad = new Quad(device);

            var shadowCameraEntity = kernel.Get<EntityDescription>();
            shadowCameraEntity.AddBehaviour<View>();
            shadowView = shadowCameraEntity.Create().GetBehaviour<View>();
            shadowView.Camera = new Camera();

            lights = new List<LightData>();
            touchesNearPlane = new List<LightData>();
            touchesFarPlane = new List<LightData>();
            touchesBothPlanes = new List<LightData>();
            touchesNeitherPlane = new List<LightData>();

            depthGreater = new DepthStencilState()
            {
                DepthBufferEnable = true,
                DepthBufferWriteEnable = false,
                DepthBufferFunction = CompareFunction.GreaterEqual
            };

            stencilWritePass = new DepthStencilState()
            {
                DepthBufferEnable = true,
                DepthBufferWriteEnable = false,
                DepthBufferFunction = CompareFunction.LessEqual,
                StencilEnable = true,
                TwoSidedStencilMode = true,
                StencilFunction = CompareFunction.Always,
                StencilPass = StencilOperation.Increment,
                CounterClockwiseStencilPass = StencilOperation.Decrement
            };

            stencilCheckPass = new DepthStencilState()
            {
                DepthBufferEnable = false,
                StencilEnable = true,
                StencilFunction = CompareFunction.NotEqual,
                ReferenceStencil = 0
            };

            colourWriteDisable = new BlendState()
            {
                ColorWriteChannels = ColorWriteChannels.None
            };
        }

        public override void Add(SpotLight behaviour)
        {
            var data = new LightData()
            {
                Light = behaviour
            };

            lights.Add(data);
            
            base.Add(behaviour);
        }

        public override bool Remove(SpotLight behaviour)
        {
            bool removed = base.Remove(behaviour);
            if (removed)
            {
                for (int i = 0; i < lights.Count; i++)
                {
                    if (lights[i].Light == behaviour)
                    {
                        lights.RemoveAt(i);
                        break;
                    }
                }
            }

            return removed;
        }

        public void Prepare(Renderer renderer)
        {
            touchesNearPlane.Clear();
            touchesFarPlane.Clear();
            touchesBothPlanes.Clear();
            touchesNeitherPlane.Clear();

            var frustum = renderer.Data.Get<BoundingFrustum>("viewfrustum").Value;

            float falloffFactor = renderer.Data.Get("lighting_attenuationscale", 100).Value;
            geometryLightingMaterial.Parameters["LightFalloffFactor"].SetValue(falloffFactor);
            quadLightingMaterial.Parameters["LightFalloffFactor"].SetValue(falloffFactor);

            //float threshold = renderer.Data.Get("lighting_threshold", 1f / 100f).Value;
            //float adaptedLuminance = renderer.Data.Get<float>("adaptedluminance", 1).Value;

            //threshold = adaptedLuminance * threshold;

            foreach (var light in lights)
            {
                if (!light.Light.Active)
                    continue;

                light.Light.Direction = Vector3.Normalize(light.Light.Direction);

                //var luminance = Math.Max(light.Colour.X, Math.Max(light.Colour.Y, light.Colour.Z));
                //light.range = (float)Math.Sqrt(luminance * falloffFactor / threshold);

                var bounds = new BoundingSphere(light.Light.Position, light.Light.Range);
                if (!bounds.Intersects(frustum))
                    continue;

                var near = bounds.Intersects(frustum.Near) == PlaneIntersectionType.Intersecting;
                var far = bounds.Intersects(frustum.Far) == PlaneIntersectionType.Intersecting;

                if (near && far)
                    touchesBothPlanes.Add(light);
                else if (near)
                    touchesNearPlane.Add(light);
                else if (far)
                    touchesFarPlane.Add(light);
                else
                    touchesNeitherPlane.Add(light);

                if (light.ShadowMap != null)
                {
                    RenderTargetManager.RecycleTarget(light.ShadowMap);
                    light.ShadowMap = null;
                }

                if (light.Light.ShadowResolution > 0)
                    DrawShadowMap(renderer, light);
            }
        }

        private void DrawShadowMap(Renderer renderer, LightData data)
        {
            var light = data.Light;

            var target = RenderTargetManager.GetTarget(renderer.Device, light.ShadowResolution, light.ShadowResolution, SurfaceFormat.Single, DepthFormat.Depth24Stencil8, name: "spot light shadow map");
            renderer.Device.SetRenderTarget(target);
            renderer.Device.Clear(Color.Black);

            var resolution = renderer.Data.Get<Vector2>("resolution");
            var previousResolution = resolution.Value;
            resolution.Value = new Vector2(light.ShadowResolution);

            renderer.Device.DepthStencilState = DepthStencilState.Default;
            renderer.Device.BlendState = BlendState.Opaque;
            renderer.Device.RasterizerState = RasterizerState.CullCounterClockwise;

            var view = renderer.Data.Get<View>("activeview");
            var previousView = view.Value;
            view.Value = shadowView;

            data.View = Matrix.CreateLookAt(
                light.Position,
                light.Position + light.Direction,
                light.Direction == Vector3.Up || light.Direction == Vector3.Down ? Vector3.Right : Vector3.Up);
            data.Projection = Matrix.CreatePerspectiveFieldOfView(light.Angle, 1, 1, light.Range);

            shadowView.Camera.View = data.View;
            shadowView.Camera.Projection = data.Projection;
            shadowView.Camera.NearClip = 1;
            shadowView.Camera.FarClip = light.Range;
            shadowView.Viewport = new Viewport(0, 0, light.ShadowResolution, light.ShadowResolution);
            shadowView.SetMetadata(renderer.Data);

            foreach (var item in renderer.Scene.FindManagers<IGeometryProvider>())
                item.Draw("shadows_viewlength", renderer.Data);

            data.ShadowMap = target;
            resolution.Value = previousResolution;
            previousView.SetMetadata(renderer.Data);
            view.Value = previousView;
        }

        public void Draw(Renderer renderer)
        {
            var metadata = renderer.Data;
            var device = renderer.Device;

            var part = geometry.Meshes[0].MeshParts[0];
            device.SetVertexBuffer(part.VertexBuffer);
            device.Indices = part.IndexBuffer;

            DrawGeometryLights(touchesFarPlane, metadata, device);

            device.DepthStencilState = depthGreater;
            device.RasterizerState = RasterizerState.CullClockwise;
            DrawGeometryLights(touchesNearPlane, metadata, device);
            DrawGeometryLights(touchesNeitherPlane, metadata, device);

            //foreach (var light in touchesNeitherPlane)
            //{
            //    device.DepthStencilState = stencilWritePass;
            //    device.RasterizerState = RasterizerState.CullNone;
            //    device.BlendState = colourWriteDisable;
            //    device.Clear(ClearOptions.Stencil, Color.Transparent, 0, 0);

            //    SetupLight(metadata, null, light);
            //    DrawGeomery(nothingMaterial, metadata, device);

            //    device.DepthStencilState = stencilCheckPass;
            //    device.RasterizerState = RasterizerState.CullCounterClockwise;
            //    device.BlendState = BlendState.Additive;
            //    DrawGeomery(geometryLightingMaterial, metadata, device);
            //}

            device.DepthStencilState = DepthStencilState.None;
            device.RasterizerState = RasterizerState.CullCounterClockwise;

            foreach (var light in touchesBothPlanes)
            {
                SetupLight(metadata, quadLightingMaterial, light);
                quad.Draw(quadLightingMaterial, metadata);
            }
        }

        private void DrawGeometryLights(List<LightData> lights, RendererMetadata metadata, GraphicsDevice device)
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

        private void SetupLight(RendererMetadata metadata, Material material, LightData data)
        {
            var light = data.Light;

            if (material != null)
            {
                Matrix view = metadata.Get<Matrix>("view").Value;
                Vector3 position = light.Position;
                Vector3 direction = light.Direction;
                Vector3.Transform(ref position, ref view, out position);
                Vector3.TransformNormal(ref direction, ref view, out direction);
                float angle = (float)Math.Cos(light.Angle / 2);

                if (light.Mask != null || light.ShadowResolution > 0)
                {
                    var inverseView = metadata.Get<Matrix>("inverseview").Value;
                    var cameraToLightProjection = inverseView * data.View * data.Projection;
                    material.Parameters["CameraViewToLightProjection"].SetValue(cameraToLightProjection);
                }

                material.Parameters["LightPosition"].SetValue(position);
                material.Parameters["LightDirection"].SetValue(-direction);
                material.Parameters["Angle"].SetValue(angle);
                material.Parameters["Range"].SetValue(light.Range);
                material.Parameters["Colour"].SetValue(light.Colour);
                material.Parameters["EnableProjectiveTexturing"].SetValue(light.Mask != null);
                material.Parameters["Mask"].SetValue(light.Mask);
                material.Parameters["EnableShadows"].SetValue(light.ShadowResolution > 0);
                material.Parameters["ShadowMapSize"].SetValue(new Vector2(light.ShadowResolution, light.ShadowResolution));
                material.Parameters["ShadowMap"].SetValue(data.ShadowMap);
                material.Parameters["LightFarClip"].SetValue(light.Range);

                var nearPlane = new Plane(light.Direction, Vector3.Dot(light.Direction, light.Position));
                nearPlane.Normalize();
                nearPlane = Plane.Transform(nearPlane, view);
                material.Parameters["LightNearPlane"].SetValue(new Vector4(nearPlane.Normal, nearPlane.D));
            }

            var world = Matrix.CreateScale(light.Range / geometry.Meshes[0].BoundingSphere.Radius)
                        * Matrix.CreateTranslation(light.Position);
            metadata.Set<Matrix>("world", world);
            Matrix.Multiply(ref world, ref metadata.Get<Matrix>("view").Value, out metadata.Get<Matrix>("worldview").Value);
            Matrix.Multiply(ref metadata.Get<Matrix>("worldview").Value, ref metadata.Get<Matrix>("projection").Value, out metadata.Get<Matrix>("worldviewprojection").Value);
        }
    }
}
