using System;
using System.Collections.Generic;
using System.Numerics;
using Microsoft.Xna.Framework.Graphics;
using Myre.Entities;
using Myre.Entities.Behaviours;
using Myre.Extensions;
using Myre.Graphics.Geometry;
using Myre.Graphics.Lighting;
using Myre.Graphics.Materials;
using Ninject;
using SwizzleMyVectors.Geometry;
using Color = Microsoft.Xna.Framework.Color;

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
            public Matrix4x4 View;
            public Matrix4x4 Projection;
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
            _shadowView = shadowCameraEntity.Create().GetBehaviour<View>(null);
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
                if (!light.Light.Active)
                    continue;

                SetupLight(renderer.Data, light);
                _quad.Draw(_material, renderer.Data);
            }
        }

        private void SetupLight(RendererMetadata metadata, LightData data)
        {
            var light = data.Light;

            if (_material != null)
            {
                Matrix4x4 view = metadata.GetValue(new TypedName<Matrix4x4>("view"));
                Vector3 direction = Vector3.TransformNormal(light.Direction, view);

                var shadowsEnabled = light.ShadowResolution > 0;

                _material.Parameters["Direction"].SetValue(direction);
                _material.Parameters["Colour"].SetValue(light.Colour);
                _material.Parameters["EnableShadows"].SetValue(shadowsEnabled);

                if (shadowsEnabled)
                {
                    _material.Parameters["ShadowProjection"].SetValue(metadata.GetValue(new TypedName<Matrix4x4>("inverseview")) * data.View * data.Projection);
                    _material.Parameters["ShadowMapSize"].SetValue(new Vector2(light.ShadowResolution, light.ShadowResolution));
                    _material.Parameters["ShadowMap"].SetValue(data.ShadowMap);
                    _material.Parameters["LightFarClip"].SetValue(data.FarClip);
                    _material.Parameters["LightNearPlane"].SetValue(data.NearClip);
                }
            }
        }

        public void Prepare(Renderer renderer)
        {
            renderer.Data.GetValue(new TypedName<BoundingFrustum>("viewfrustum")).GetCorners(_frustumCornersWs);

            for (int i = 0; i < _lights.Count; i++)
            {
                var data = _lights[i];
                var light = data.Light;

                if (!light.Active)
                    continue;

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
            var lightIsVertical = light.Direction == Vector3.UnitY || light.Direction == -Vector3.UnitY;
            var viewMatrix = Matrix4x4.CreateLookAt(lightPosition, Vector3.Zero, lightIsVertical ? -Vector3.UnitZ : Vector3.UnitY);

            for (int i = 0; i < _frustumCornersWs.Length; i++)
                _frustumCornersVs[i] = Vector3.Transform(_frustumCornersWs[i], viewMatrix);

            var bounds = BoundingSphere.CreateFromPoints(_frustumCornersVs);

            var farClip = max - min;
            var projectionMatrix = Matrix4x4.CreateOrthographicOffCenter(-bounds.Radius, bounds.Radius, -bounds.Radius, bounds.Radius, 0, farClip);

            data.View = viewMatrix;
            data.Projection = projectionMatrix;
            data.FarClip = farClip;

            var nearPlane = new Plane(light.Direction, depthOffset);
            nearPlane = Plane.Normalize(nearPlane);
            var view = renderer.Data.GetValue(new TypedName<Matrix4x4>("view"));
            Plane transformedNearPlane = Plane.Transform(nearPlane, view);
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
