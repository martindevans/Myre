using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Myre.Graphics.Materials;
using Microsoft.Xna.Framework.Graphics;
using Myre.Entities.Behaviours;
using Myre.Graphics.Lighting;
using Ninject;
using Microsoft.Xna.Framework.Content;
using Myre.Entities;
using Myre.Graphics.Geometry;

namespace Myre.Graphics.Deferred.LightManagers
{
    public class DeferredSunLightManager
           : BehaviourManager<SunLight>, IDirectLight
    {
        class LightData
        {
            public SunLight Light;
            public RenderTarget2D ShadowMap;
            public Vector4 NearClip;
            public float FarClip;
            public Matrix View;
            public Matrix Projection;
        }

        private Material material;
        private Quad quad;
        private List<ICullable> visibilityResults;

        private List<LightData> lights;

        private Vector3[] frustumCornersWS;
        private Vector3[] frustumCornersVS;
        private View shadowView;

        public DeferredSunLightManager(IKernel kernel, GraphicsDevice device)
        {
            var effect = Content.Load<Effect>("DirectionalLight");
            material = new Material(effect, null);

            quad = new Quad(device);
            visibilityResults = new List<ICullable>();

            lights = new List<LightData>();

            frustumCornersWS = new Vector3[8];
            frustumCornersVS = new Vector3[8];

            var shadowCameraEntity = kernel.Get<EntityDescription>();
            shadowCameraEntity.AddBehaviour<View>();
            shadowView = shadowCameraEntity.Create().GetBehaviour<View>();
            shadowView.Camera = new Camera();
        }

        public override void Add(SunLight behaviour)
        {
            lights.Add(new LightData()
            {
                Light = behaviour,
            });

            base.Add(behaviour);
        }

        public override bool Remove(SunLight behaviour)
        {
            if (base.Remove(behaviour))
            {
                for (int i = 0; i < lights.Count; i++)
                {
                    if (lights[i].Light == behaviour)
                    {
                        lights.RemoveAt(i);
                        return true;
                    }
                }
            }
            return false;
        }

        public void Draw(Renderer renderer)
        {
            foreach (var light in lights)
            {
                SetupLight(renderer.Data, light);
                quad.Draw(material, renderer.Data);
            }
        }

        private void SetupLight(RendererMetadata metadata, LightData data)
        {
            var light = data.Light;

            if (material != null)
            {
                Matrix view = metadata.Get<Matrix>("view").Value;
                Vector3 direction = light.Direction;
                Vector3.TransformNormal(ref direction, ref view, out direction);

                var shadowsEnabled = light.ShadowResolution > 0;

                material.Parameters["Direction"].SetValue(direction);
                material.Parameters["Colour"].SetValue(light.Colour);
                material.Parameters["EnableShadows"].SetValue(shadowsEnabled);

                if (shadowsEnabled)
                {
                    material.Parameters["ShadowProjection"].SetValue(metadata.Get<Matrix>("inverseview").Value * data.View * data.Projection);
                    material.Parameters["ShadowMapSize"].SetValue(new Vector2(light.ShadowResolution, light.ShadowResolution));
                    material.Parameters["ShadowMap"].SetValue(data.ShadowMap);
                    material.Parameters["LightFarClip"].SetValue(data.FarClip);
                    material.Parameters["LightNearPlane"].SetValue(data.NearClip);
                }
            }
        }

        public void Prepare(Renderer renderer)
        {
            renderer.Data.Get<BoundingFrustum>("viewfrustum").Value.GetCorners(frustumCornersWS);

            for (int i = 0; i < lights.Count; i++)
            {
                var data = lights[i];
                var light = data.Light;

                light.Direction = Vector3.Normalize(light.Direction);

                if (data.ShadowMap != null)
                {
                    RenderTargetManager.RecycleTarget(data.ShadowMap);
                    data.ShadowMap = null;
                }

                if (light.ShadowResolution != 0)
                {
                    CalculateShadowMatrices(renderer, data);
                    DrawShadowMap(renderer, data);
                }
            }
        }

        private void CalculateShadowMatrices(Renderer renderer, LightData data)
        {
            var light = data.Light;

            var min = float.PositiveInfinity;
            var max = float.NegativeInfinity;
            for (int i = 0; i < frustumCornersWS.Length; i++)
            {
                var projection = Vector3.Dot(frustumCornersWS[i], light.Direction);
                min = Math.Min(min, projection);
                max = Math.Max(max, projection);
            }

            min = -500;
            max = 500;

            var depthOffset = -min;
            var lightPosition = -light.Direction * depthOffset;
            var lightIsVertical = light.Direction == Vector3.Up || light.Direction == Vector3.Down;
            var viewMatrix = Matrix.CreateLookAt(lightPosition, Vector3.Zero, lightIsVertical ? Vector3.Forward : Vector3.Up);

            Vector3.Transform(frustumCornersWS, ref viewMatrix, frustumCornersVS);

            var bounds = BoundingSphere.CreateFromPoints(frustumCornersVS);

            var farClip = max - min;
            var projectionMatrix = Matrix.CreateOrthographicOffCenter(-bounds.Radius, bounds.Radius, -bounds.Radius, bounds.Radius, 0, farClip);

            data.View = viewMatrix;
            data.Projection = projectionMatrix;
            data.FarClip = farClip;

            var nearPlane = new Plane(light.Direction, depthOffset);
            nearPlane.Normalize();
            Plane transformedNearPlane;
            Plane.Transform(ref nearPlane, ref renderer.Data.Get<Matrix>("view").Value, out transformedNearPlane);
            data.NearClip = new Vector4(transformedNearPlane.Normal, transformedNearPlane.D);
        }

        private void DrawShadowMap(Renderer renderer, LightData data)
        {
            var light = data.Light;

            var target = RenderTargetManager.GetTarget(renderer.Device, light.ShadowResolution, light.ShadowResolution, SurfaceFormat.Single, DepthFormat.Depth24Stencil8, name: "sun light shadow map");
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

            shadowView.Camera.View = data.View;
            shadowView.Camera.Projection = data.Projection;
            shadowView.Camera.NearClip = 1;
            shadowView.Camera.FarClip = data.FarClip;
            shadowView.Viewport = new Viewport(0, 0, light.ShadowResolution, light.ShadowResolution);
            shadowView.SetMetadata(renderer.Data);

            foreach (var item in renderer.Scene.FindManagers<IGeometryProvider>())
                item.Draw("shadows_viewz", renderer.Data);

            data.ShadowMap = target;
            resolution.Value = previousResolution;
            previousView.SetMetadata(renderer.Data);
            view.Value = previousView;
        }
    }
}
