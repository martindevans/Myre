using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Myre.Entities;
using Myre.Entities.Behaviours;
using Myre.Graphics.Geometry;
using Myre.Graphics.Lighting;
using Myre.Graphics.Materials;
using Ninject;

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

        private readonly Material _material;
        private readonly Quad _quad;

        private readonly List<LightData> _lights;

        private readonly Vector3[] _frustumCornersWs;
        private readonly Vector3[] _frustumCornersVs;
        private readonly View _shadowView;

        public DeferredSunLightManager(IKernel kernel, GraphicsDevice device)
        {
            var effect = Content.Load<Effect>("DirectionalLight");
            _material = new Material(effect, null);

            _quad = new Quad(device);

            _lights = new List<LightData>();

            _frustumCornersWs = new Vector3[8];
            _frustumCornersVs = new Vector3[8];

            var shadowCameraEntity = kernel.Get<EntityDescription>();
            shadowCameraEntity.AddBehaviour<View>();
            _shadowView = shadowCameraEntity.Create().GetBehaviour<View>();
            _shadowView.Camera = new Camera();
        }

        public override void Add(SunLight behaviour)
        {
            _lights.Add(new LightData()
            {
                Light = behaviour,
            });

            base.Add(behaviour);
        }

        public override bool Remove(SunLight behaviour)
        {
            if (base.Remove(behaviour))
            {
                for (int i = 0; i < _lights.Count; i++)
                {
                    if (_lights[i].Light == behaviour)
                    {
                        _lights.RemoveAt(i);
                        return true;
                    }
                }
            }
            return false;
        }

        public void Draw(Renderer renderer)
        {
            foreach (var light in _lights)
            {
                SetupLight(renderer.Data, light);
                _quad.Draw(_material, renderer.Data);
            }
        }

        private void SetupLight(RendererMetadata metadata, LightData data)
        {
            var light = data.Light;

            if (_material != null)
            {
                Matrix view = metadata.GetValue<Matrix>("view");
                Vector3 direction = light.Direction;
                Vector3.TransformNormal(ref direction, ref view, out direction);

                var shadowsEnabled = light.ShadowResolution > 0;

                _material.Parameters["Direction"].SetValue(direction);
                _material.Parameters["Colour"].SetValue(light.Colour);
                _material.Parameters["EnableShadows"].SetValue(shadowsEnabled);

                if (shadowsEnabled)
                {
                    _material.Parameters["ShadowProjection"].SetValue(metadata.GetValue<Matrix>("inverseview") * data.View * data.Projection);
                    _material.Parameters["ShadowMapSize"].SetValue(new Vector2(light.ShadowResolution, light.ShadowResolution));
                    _material.Parameters["ShadowMap"].SetValue(data.ShadowMap);
                    _material.Parameters["LightFarClip"].SetValue(data.FarClip);
                    _material.Parameters["LightNearPlane"].SetValue(data.NearClip);
                }
            }
        }

        public void Prepare(Renderer renderer)
        {
            renderer.Data.GetValue<BoundingFrustum>("viewfrustum").GetCorners(_frustumCornersWs);

            for (int i = 0; i < _lights.Count; i++)
            {
                var data = _lights[i];
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
            for (int i = 0; i < _frustumCornersWs.Length; i++)
            {
                var projection = Vector3.Dot(_frustumCornersWs[i], light.Direction);
                min = Math.Min(min, projection);
                max = Math.Max(max, projection);
            }

            min = -500;
            max = 500;

            var depthOffset = -min;
            var lightPosition = -light.Direction * depthOffset;
            var lightIsVertical = light.Direction == Vector3.Up || light.Direction == Vector3.Down;
            var viewMatrix = Matrix.CreateLookAt(lightPosition, Vector3.Zero, lightIsVertical ? Vector3.Forward : Vector3.Up);

            Vector3.Transform(_frustumCornersWs, ref viewMatrix, _frustumCornersVs);

            var bounds = BoundingSphere.CreateFromPoints(_frustumCornersVs);

            var farClip = max - min;
            var projectionMatrix = Matrix.CreateOrthographicOffCenter(-bounds.Radius, bounds.Radius, -bounds.Radius, bounds.Radius, 0, farClip);

            data.View = viewMatrix;
            data.Projection = projectionMatrix;
            data.FarClip = farClip;

            var nearPlane = new Plane(light.Direction, depthOffset);
            nearPlane.Normalize();
            Plane transformedNearPlane;
            var view = renderer.Data.GetValue<Matrix>("view");
            Plane.Transform(ref nearPlane, ref view, out transformedNearPlane);
            data.NearClip = new Vector4(transformedNearPlane.Normal, transformedNearPlane.D);
        }

        private void DrawShadowMap(Renderer renderer, LightData data)
        {
            var light = data.Light;

            var target = RenderTargetManager.GetTarget(renderer.Device, light.ShadowResolution, light.ShadowResolution, SurfaceFormat.Single, DepthFormat.Depth24Stencil8, name: "sun light shadow map");
            renderer.Device.SetRenderTarget(target);
            renderer.Device.Clear(Color.Black);

            var resolution = renderer.Data.Get<Vector2>("resolution", default(Vector2), true);
            var previousResolution = resolution.Value;
            resolution.Value = new Vector2(light.ShadowResolution);

            renderer.Device.DepthStencilState = DepthStencilState.Default;
            renderer.Device.BlendState = BlendState.Opaque;
            renderer.Device.RasterizerState = RasterizerState.CullCounterClockwise;

            var view = renderer.Data.Get<View>("activeview", default(View), true);
            var previousView = view.Value;
            view.Value = _shadowView;

            _shadowView.Camera.View = data.View;
            _shadowView.Camera.Projection = data.Projection;
            _shadowView.Camera.NearClip = 1;
            _shadowView.Camera.FarClip = data.FarClip;
            _shadowView.Viewport = new Viewport(0, 0, light.ShadowResolution, light.ShadowResolution);
            _shadowView.SetMetadata(renderer.Data);

            foreach (var item in renderer.Scene.FindManagers<IGeometryProvider>())
                item.Draw("shadows_viewz", renderer.Data);

            data.ShadowMap = target;
            resolution.Value = previousResolution;
            previousView.SetMetadata(renderer.Data);
            view.Value = previousView;
        }
    }
}
