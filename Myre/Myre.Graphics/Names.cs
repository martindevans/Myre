using System.Numerics;
using Microsoft.Xna.Framework.Graphics;
using SwizzleMyVectors.Geometry;

namespace Myre.Graphics
{
    public static class Names
    {
        
        public static readonly TypedName<float> TimeDelta = new TypedName<float>("timedelta");

        public static class View
        {
            public static readonly TypedName<Graphics.View> ActiveView = new TypedName<Graphics.View>("activeview");
            public static readonly TypedName<Vector2> Resolution = new TypedName<Vector2>("resolution");
            public static readonly TypedName<BoundingFrustum> ViewFrustum = new TypedName<BoundingFrustum>("viewfrustum");
            public static readonly TypedName<Viewport> ViewPort = new TypedName<Viewport>("viewport");
            public static readonly TypedName<float> AspectRatio = new TypedName<float>("aspectratio");
            public static readonly TypedName<Camera> Camera = new TypedName<Camera>("camera");

            public static readonly TypedName<float> NearClip = new TypedName<float>("nearclip");
            public static readonly TypedName<float> FarClip = new TypedName<float>("farclip");
            public static readonly TypedName<Vector3> CameraPosition = new TypedName<Vector3>("cameraposition");
            public static readonly TypedName<Vector3[]> FarFrustumCorners = new TypedName<Vector3[]>("farfrustumcorners");
        }

        public static class Animation
        {
            public static readonly TypedName<Matrix4x4[]> Bones = new TypedName<Matrix4x4[]>("bones");
        }

        public static class Lighting
        {
            public static readonly TypedName<float> AttenuationScale = new TypedName<float>("lighting_attenuationscale");

            public static class SSAO
            {
                public static readonly TypedName<bool> Enabled = new TypedName<bool>("ssao");
            }
        }

        public static class Deferred
        {
            public static class Textures
            {
                public static readonly TypedName<Texture2D> Depth = new TypedName<Texture2D>("gbuffer_depth");
                public static readonly TypedName<Texture2D> Normals = new TypedName<Texture2D>("gbuffer_normals");
                public static readonly TypedName<Texture2D> Diffuse = new TypedName<Texture2D>("gbuffer_diffuse");
                public static readonly TypedName<Texture2D> LightBuffer = new TypedName<Texture2D>("lightbuffer");
                public static readonly TypedName<Texture2D> SSAO = new TypedName<Texture2D>("ssao");
            }
        }

        public static class Matrix
        {
            public static readonly TypedName<Matrix4x4> World = new TypedName<Matrix4x4>("world");
            public static readonly TypedName<Matrix4x4> View = new TypedName<Matrix4x4>("view");
            public static readonly TypedName<Matrix4x4> Projection = new TypedName<Matrix4x4>("projection");

            public static readonly TypedName<Matrix4x4> WorldView = new TypedName<Matrix4x4>("worldview");
            public static readonly TypedName<Matrix4x4> ViewProjection = new TypedName<Matrix4x4>("viewprojection");
            public static readonly TypedName<Matrix4x4> WorldViewProjection = new TypedName<Matrix4x4>("worldviewprojection");

            public static readonly TypedName<Matrix4x4> InverseView = new TypedName<Matrix4x4>("inverseview");
            public static readonly TypedName<Matrix4x4> InverseProjection = new TypedName<Matrix4x4>("inverseprojection");

            public static readonly TypedName<Matrix4x4> InverseViewProjection = new TypedName<Matrix4x4>("inverseviewprojection");
        }

        public static class Translucency
        {
            public static class Deferred
            {
                public static readonly TypedName<int> DepthPeelLayers = new TypedName<int>("transparency_deferred_layers");
                public static readonly TypedName<Texture2D> TransparencyLightbuffer = new TypedName<Texture2D>("transparency_lightbuffer");
            }
        }
    }
}
