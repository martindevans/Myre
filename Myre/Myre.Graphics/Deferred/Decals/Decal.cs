using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Myre.Collections;
using Myre.Debugging.Statistics;
using Myre.Entities;
using Myre.Entities.Behaviours;
using Myre.Entities.Extensions;
using Myre.Graphics.Geometry;
using Myre.Graphics.Materials;
using Ninject;

namespace Myre.Graphics.Deferred.Decals
{
    [DefaultManager(typeof(Manager))]
    public class Decal
        : Behaviour
    {
        public static readonly TypedName<Matrix> TransformName = ModelInstance.TransformName;
        public static readonly TypedName<Texture2D> DiffuseName = new TypedName<Texture2D>("diffuse_texture");
        public static readonly TypedName<Texture2D> NormalName = new TypedName<Texture2D>("normal_texture");
        public static readonly TypedName<float> AngleCutoffName = new TypedName<float>("angle_cutoff");
        public static readonly TypedName<bool> TemporaryName = new TypedName<bool>("temporary");

        private Vector3 _decalDirection;

        private Matrix _inverseTransform;

        private Property<Matrix> _transform;
        public Matrix Transform
        {
            get
            {
                return _transform.Value;
            }
            set
            {
                _transform.Value = value;
            }
        }

        private Property<Texture2D> _diffuse; 
        public Texture2D Diffuse
        {
            get
            {
                return _diffuse.Value;
            }
            set
            {
                _diffuse.Value = value;
            }
        }

        private Property<Texture2D> _normal;
        public Texture2D Normal
        {
            get
            {
                return _normal.Value;
            }
            set
            {
                _normal.Value = value;
            }
        }

        private Property<float> _angleCutoff; 
        public float AngleCutoff
        {
            get
            {
                return _angleCutoff.Value;
            }
            set
            {
                _angleCutoff.Value = value;
            }
        }

        private bool _temporary = true;
        public bool Temporary
        {
            get { return _temporary; }
        }

        public override void CreateProperties(Entity.ConstructionContext context)
        {
            _transform = context.CreateProperty(TransformName);
            _transform.PropertySet += (p, o, n) => {
                _inverseTransform = Matrix.Invert(n);
                _decalDirection = Vector3.TransformNormal(Vector3.Up, n);
            };

            _diffuse = context.CreateProperty(DiffuseName);
            _normal = context.CreateProperty(NormalName);
            _angleCutoff = context.CreateProperty(AngleCutoffName, MathHelper.Pi);

            base.CreateProperties(context);
        }

        public override void Initialise(INamedDataProvider initialisationData)
        {
            base.Initialise(initialisationData);

            initialisationData.TryCopyValue(_transform);
            initialisationData.TryCopyValue(_diffuse);
            initialisationData.TryCopyValue(_normal);
            if (!initialisationData.TryCopyValue(_angleCutoff))
                _angleCutoff.Value = MathHelper.Pi;
            initialisationData.TryCopyValue(TemporaryName, ref _temporary);
        }

        public class Manager
            : BehaviourManager<Decal>
        {
            private VertexBuffer _unitCube;
            private IndexBuffer _unitCubeIndices;

            private Material _zTestedDecalDiffuseMaterial;
            private Material _zTestedDecalNormalMaterial;
            private Material _zTestedDecalCombinedMaterial;

            private Material _decalDiffuseMaterial;
            private Material _decalNormalMaterial;
            private Material _decalCombinedMaterial;

            public override void Initialise(Scene scene)
            {
                base.Initialise(scene);

                _unitCube = new VertexBuffer(scene.Kernel.Get<GraphicsDevice>(), typeof(VertexPosition), 8, BufferUsage.WriteOnly);
                _unitCube.SetData(new VertexPosition[] {
                    new VertexPosition(-0.5f, -0.5f, -0.5f),
                    new VertexPosition(-0.5f, 0.5f, -0.5f),
                    new VertexPosition(0.5f, 0.5f, -0.5f),
                    new VertexPosition(0.5f, -0.5f, -0.5f),
                    new VertexPosition(-0.5f, -0.5f, 0.5f),
                    new VertexPosition(-0.5f, 0.5f, 0.5f),
                    new VertexPosition(0.5f, 0.5f, 0.5f),
                    new VertexPosition(0.5f, -0.5f, 0.5f),
                });

                _unitCubeIndices = new IndexBuffer(scene.Kernel.Get<GraphicsDevice>(), IndexElementSize.SixteenBits, 36, BufferUsage.WriteOnly);
                _unitCubeIndices.SetData(new ushort[] {
                    7, 3, 4,
                    3, 0, 4,
                    2, 6, 1,
                    6, 5, 1,
                    7, 6, 3,
                    6, 2, 3,
                    0, 1, 4,
                    1, 5, 4,
                    6, 7, 4,
                    5, 6, 4,
                    3, 2, 0,
                    2, 1, 0,
                });

                _zTestedDecalDiffuseMaterial = new Material(Content.Load<Effect>("Decal").Clone(), "ZTestedDecalDiffuse");
                _zTestedDecalNormalMaterial = new Material(Content.Load<Effect>("Decal").Clone(), "ZTestedDecalNormal");
                _zTestedDecalCombinedMaterial = new Material(Content.Load<Effect>("Decal").Clone(), "ZTestedDecalDiffuseNormal");

                _decalDiffuseMaterial = new Material(Content.Load<Effect>("Decal").Clone(), "DecalDiffuse");
                _decalNormalMaterial = new Material(Content.Load<Effect>("Decal").Clone(), "DecalNormal");
                _decalCombinedMaterial = new Material(Content.Load<Effect>("Decal").Clone(), "DecalDiffuseNormal");
            }

#if PROFILE
            private static readonly Statistic _polysDrawnStat = Statistic.Create("Graphics.Primitives");
            private static readonly Statistic _drawsStat = Statistic.Create("Graphics.Draws");
#endif

            public int MaxTemporaryDecals { get; set; }

            private int _temporaryCount = 0;

            public Manager()
            {
                MaxTemporaryDecals = 64;
            }

            public override void Add(Decal behaviour)
            {
                base.Add(behaviour);

                if (behaviour.Temporary)
                    _temporaryCount++;

                if (MaxTemporaryDecals > 0 && MaxTemporaryDecals < _temporaryCount)
                {
                    var tmp = Behaviours.Find(a => a.Temporary);
                    if (tmp != null)
                        Remove(tmp);
                }
            }

            public override bool Remove(Decal behaviour)
            {
                if (!base.Remove(behaviour))
                    return false;

                if (behaviour.Temporary)
                    _temporaryCount--;

                return true;
            }

            public void Draw(Renderer renderer)
            {
                var device = renderer.Device;

                //Set geometry onto the device
                device.SetVertexBuffer(_unitCube);
                device.Indices = _unitCubeIndices;

                //Get data from the renderer
                var worldBox = renderer.Data.Get<Matrix>("world");
                var inverseWorldBox = renderer.Data.Get<Matrix>("inverseworld");
                var cameraViewBox = renderer.Data.Get<Matrix>("view");
                var worldViewBox = renderer.Data.Get<Matrix>("worldview");

                var cameraPosition = renderer.Data.Get<Vector3>("cameraposition");
                var cameraNearClip = renderer.Data.Get<float>("nearclip");

                var viewFrustum = renderer.Data.GetValue(new TypedName<BoundingFrustum>("viewfrustum"));

                foreach (var behaviour in Behaviours)
                {
                    BoundingSphere b = new BoundingSphere(Vector3.Zero, 1).Transform(behaviour.Transform);
                    if (!viewFrustum.Intersects(b))
                        continue;

                    //Setup transforms
                    worldBox.Value = behaviour.Transform;
                    inverseWorldBox.Value = behaviour._inverseTransform;
                    worldViewBox.Value = behaviour.Transform * cameraViewBox.Value;

                    //Transform the camera position into object space and test to see if it intersects the decal
                    var cameraSphere = new BoundingSphere(cameraPosition.Value, cameraNearClip.Value).Transform(behaviour._inverseTransform);
                    var cameraIntersectsDecal = new BoundingBox(new Vector3(-0.5f), new Vector3(0.5f)).Contains(cameraSphere) != ContainmentType.Disjoint;

                    //Select appropriate material
                    Material material = SelectMaterial(behaviour, !cameraIntersectsDecal);
                    if (material == null)
                        continue;

                    //Set shader parameters
                    if (behaviour.Diffuse != null)
                        material.Parameters["Diffuse"].SetValue(behaviour.Diffuse);
                    if (behaviour.Normal != null)
                        material.Parameters["Normal"].SetValue(behaviour.Normal);
                    material.Parameters["DecalDirection"].SetValue(behaviour._decalDirection);
                    material.Parameters["DecalDirectionClip"].SetValue((float) Math.Cos(behaviour.AngleCutoff));

                    if (cameraIntersectsDecal)
                        renderer.Device.RasterizerState = RasterizerState.CullClockwise;
                    else
                        renderer.Device.RasterizerState = RasterizerState.CullCounterClockwise;

                    //Draw geometry
                    foreach (var pass in material.Begin(renderer.Data))
                    {
                        pass.Apply();

                        device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, _unitCube.VertexCount, 0, _unitCubeIndices.IndexCount / 3);

#if PROFILE
                        _drawsStat.Add(1);
#endif
                    }

#if PROFILE
                    _polysDrawnStat.Add(_unitCubeIndices.IndexCount / 3f);
#endif
                }

                renderer.Device.RasterizerState = RasterizerState.CullCounterClockwise;
            }

            private Material SelectMaterial(Decal behaviour, bool zTest)
            {
                if (behaviour.Diffuse == null)
                {
                    if (behaviour.Normal == null)
                        return null;

                    return zTest ? _zTestedDecalNormalMaterial : _decalNormalMaterial;
                }
                else
                {
                    if (behaviour.Normal != null)
                        return zTest ? _zTestedDecalCombinedMaterial : _decalCombinedMaterial;
                    else
                        return zTest ? _zTestedDecalDiffuseMaterial : _decalDiffuseMaterial;
                }
            }
        }
    }
}
