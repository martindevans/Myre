using System.Numerics;
using Microsoft.Xna.Framework.Graphics;
using Myre.Collections;
using Myre.Entities;
using Myre.Entities.Behaviours;
using Myre.Entities.Extensions;
using Myre.Graphics.Materials;
using System;
using System.Collections.Generic;
using System.Linq;
#if PROFILE
using Myre.Debugging.Statistics;
#endif
using BoundingFrustum = SwizzleMyVectors.Geometry.BoundingFrustum;
using BoundingSphere = SwizzleMyVectors.Geometry.BoundingSphere;

namespace Myre.Graphics.Geometry
{
    [DefaultManager(typeof(Manager))]
    public class ModelInstance
        : Behaviour
    {
        public static readonly TypedName<ModelData> ModelName = new TypedName<ModelData>("model");
        public static readonly TypedName<Matrix4x4> TransformName = new TypedName<Matrix4x4>("transform");
        public static readonly TypedName<bool> IsStaticName = new TypedName<bool>("is_static");
        public static readonly TypedName<bool> IsInvisibleName = new TypedName<bool>("is_invisible");
        public static readonly TypedName<float> OpacityName = new TypedName<float>("opacity");
        public static readonly TypedName<float> AttenuationName = new TypedName<float>("attenuation");
        public static readonly TypedName<float> SubSurfaceScatteringName = new TypedName<float>("subsurface_scattering");
        public static readonly TypedName<Matrix4x4?> CustomViewMatrixName = new TypedName<Matrix4x4?>("custom_view_matrix");
        public static readonly TypedName<Matrix4x4?> CustomProjectionMatrixName = new TypedName<Matrix4x4?>("custom_projection_matrix");

        private Property<ModelData> _model;
        private Property<Matrix4x4> _transform;
        private Property<bool> _isStatic;
        private Property<bool> _isInvisible;
        private Property<float> _opacity;
        private Property<float> _attenuation;
        private Property<float> _scattering;
        private Property<Matrix4x4?> _customViewMatrix;
        private Property<Matrix4x4?> _customProjectionMatrix;

        private IRenderDataSupplier[] _renderDataSuppliers;

        public ModelData Model
        {
            get { return _model.Value; }
            set { _model.Value = value; }
        }

        public Matrix4x4 Transform
        {
            get { return _transform.Value; }
            set { _transform.Value = value; }
        }

        public bool IsStatic
        {
            get { return _isStatic.Value; }
            set { _isStatic.Value = value; }
        }

        public bool IsInvisible
        {
            get { return _isInvisible.Value; }
            set { _isInvisible.Value = value; }
        }

        public float Opacity
        {
            get { return _opacity.Value; }
            set { _opacity.Value = value; }
        }

        public float Attenuation
        {
            get { return _attenuation.Value; }
            set { _attenuation.Value = value; }
        }

        public float SubSurfaceScattering
        {
            get { return _scattering.Value; }
            set { _scattering.Value = value; }
        }


        internal event Action<ModelInstance> ModelDataChanged;
        internal event Action<ModelInstance, Mesh> ModelMeshAdded;
        internal event Action<ModelInstance, Mesh> ModelMeshRemoved;

        private void MeshAdded(ModelData data, Mesh mesh)
        {
            ModelMeshAdded(this, mesh);
        }

        private void MeshRemoved(ModelData data, Mesh mesh)
        {
            ModelMeshRemoved(this, mesh);
        }

        private void HookEvents(ModelData data)
        {
            if (data == null)
                return;
            data.MeshAdded += MeshAdded;
            data.MeshRemoved += MeshRemoved;
        }

        private void UnhookEvents(ModelData data)
        {
            if (data == null)
                return;
            data.MeshAdded -= MeshAdded;
            data.MeshRemoved -= MeshRemoved;
        }

        public override void CreateProperties(Entity.ConstructionContext context)
        {
            _model = context.CreateProperty(ModelName);
            _transform = context.CreateProperty(TransformName);
            _isStatic = context.CreateProperty(IsStaticName);
            _isInvisible = context.CreateProperty(IsInvisibleName);
            _opacity = context.CreateProperty(OpacityName, 1);
            _attenuation = context.CreateProperty(AttenuationName, 1);
            _scattering = context.CreateProperty(SubSurfaceScatteringName, 0);
            _customViewMatrix = context.CreateProperty(CustomViewMatrixName);
            _customProjectionMatrix = context.CreateProperty(CustomProjectionMatrixName);

            base.CreateProperties(context);
        }

        public override void Initialise(INamedDataProvider initialisationData)
        {
            initialisationData.TryCopyValue(this, TransformName, _transform);
            initialisationData.TryCopyValue(this, IsStaticName, _isStatic);
            initialisationData.TryCopyValue(this, IsInvisibleName, _isInvisible);
            initialisationData.TryCopyValue(this, OpacityName, _opacity);
            initialisationData.TryCopyValue(this, AttenuationName, _attenuation);
            initialisationData.TryCopyValue(this, SubSurfaceScatteringName, _scattering);
            initialisationData.TryCopyValue(this, CustomViewMatrixName, _customViewMatrix);
            initialisationData.TryCopyValue(this, CustomProjectionMatrixName, _customProjectionMatrix);

            HookEvents(_model.Value);
            _model.PropertySet += (p, o, n) =>
            {
                UnhookEvents(o);
                HookEvents(n);
                if (ModelDataChanged != null)
                    ModelDataChanged(this);
            };

            _renderDataSuppliers = Owner.Behaviours.OfType<IRenderDataSupplier>().Where(s => s.Name == Name).ToArray();

            base.Initialise(initialisationData);
        }

        public override void Shutdown(INamedDataProvider shutdownData)
        {
            UnhookEvents(_model.Value);

            base.Shutdown(shutdownData);
        }

        private void ApplyRendererData(NamedBoxCollection metadata)
        {
            if (_renderDataSuppliers != null)
                for (int i = 0; i < _renderDataSuppliers.Length; i++)
                    _renderDataSuppliers[i].SetRenderData(metadata);
        }

// ReSharper disable MemberCanBePrivate.Global
        public class Manager
// ReSharper restore MemberCanBePrivate.Global
            : BehaviourManager<ModelInstance>, IGeometryProvider
        {
            class MeshInstance
                : IGeometry
            {
                public Mesh Mesh;
                public ModelInstance Instance;

                public BoundingSphere BoundingSphere { get; private set; }

                public Matrix4x4 WorldView { get; set; }
                public Matrix4x4 World { get; set; }

                public void UpdateBounds()
                {
                    BoundingSphere = Mesh.BoundingSphere.Transform(Mesh.MeshTransform * Instance.Transform);
                }

                public void Draw(string phase, Renderer renderer)
                {
                    Material material;
                    if (!Mesh.Materials.TryGetValue(phase, out material))
                        return;

                    var renderTransparent = renderer.Data.Get<bool>("render_translucent").Value;
                    if (!renderTransparent && Instance.Opacity < 1)
                        return;

                    Draw(material, renderer);
                }

                public void Draw(Material material, Renderer renderer)
                {
                    //Early exit for null meshes
                    if (Mesh.VertexBuffer == null || Mesh.IndexBuffer == null)
                        return;
                    if (Mesh.TriangleCount == 0 || Mesh.VertexCount == 0)
                        return;

                    //Set the buffers onto the device
                    renderer.Device.SetVertexBuffer(Mesh.VertexBuffer);
                    renderer.Device.Indices = Mesh.IndexBuffer;

                    //Extract useful data boxes
                    var world = renderer.Data.Get<Matrix4x4>("world", Matrix4x4.Identity);
                    var projection = renderer.Data.Get<Matrix4x4>("projection", Matrix4x4.Identity);
                    var worldView = renderer.Data.Get<Matrix4x4>("worldview", Matrix4x4.Identity);
                    var worldViewProjection = renderer.Data.Get<Matrix4x4>("worldviewprojection", Matrix4x4.Identity);

                    //We are limited in per call primitives, depending on profile. These are the max possible values for each profile
                    int maxPrimitives = renderer.Device.GraphicsProfile == GraphicsProfile.HiDef ? 1048575 : 65535;

                    //Allow the instance to apply any old data that it likes into the renderer
                    Instance.ApplyRendererData(renderer.Data);
                    renderer.Data.Set("opacity", Instance.Opacity);
                    renderer.Data.Set("attenuation", Instance.Attenuation);
                    renderer.Data.Set("scattering", Instance.SubSurfaceScattering);

                    //Calculate transform matrices
                    world.Value = World;
                    worldView.Value = WorldView;
                    if (Instance._customProjectionMatrix.Value.HasValue)
                        worldViewProjection.Value = worldView.Value * Instance._customProjectionMatrix.Value.Value;
                    else
                        worldViewProjection.Value = worldView.Value * projection.Value;

                    foreach (var pass in material.Begin(renderer.Data))
                    {
                        pass.Apply();

                        //Loop through mesh, drawing as many primitives as possible per batch
                        int primitives = Mesh.TriangleCount;
                        int offset = 0;
                        while (primitives > 0)
                        {
                            int primitiveCount = Math.Min(primitives, maxPrimitives);
                            primitives -= primitiveCount;

                            renderer.Device.DrawIndexedPrimitives(PrimitiveType.TriangleList, Mesh.BaseVertex, Mesh.MinVertexIndex, Mesh.VertexCount, Mesh.StartIndex + offset * 3, primitiveCount);

                            offset += primitiveCount;

#if PROFILE
                            _drawsStat.Add(1);
#endif
                        }

#if PROFILE
                        _polysDrawnStat.Add(Mesh.TriangleCount);
#endif
                    }
                }
            }

#if PROFILE
            private static readonly Statistic _polysDrawnStat = Statistic.Create("Graphics.Primitives");
            private static readonly Statistic _drawsStat = Statistic.Create("Graphics.Draws");
#endif

            #region fields
            private readonly Pool<MeshInstance> _meshInstancePool;
            private readonly Dictionary<string, List<MeshInstance>> _phases;
            private readonly List<MeshInstance> _buffer;
            private readonly BoundingVolume _bounds;
            #endregion

            public Manager()
            {
                _meshInstancePool = new Pool<MeshInstance>();

                _phases = new Dictionary<string, List<MeshInstance>>();
                _buffer = new List<MeshInstance>();
                _bounds = new BoundingVolume();
            }

            #region add/remove
            public override void Add(ModelInstance behaviour)
            {
                behaviour.ModelDataChanged += Changed;
                behaviour.ModelMeshAdded += AddMesh;
                behaviour.ModelMeshRemoved += RemoveMesh;

                if (behaviour.Model != null)
                    MeshesAdded(behaviour, behaviour.Model.Meshes);

                base.Add(behaviour);
            }

            private void Changed(ModelInstance instance)
            {
                Remove(instance);
                Add(instance);
            }

            private void AddMesh(ModelInstance behaviour, Mesh mesh)
            {
                var instance = _meshInstancePool.Get();
                instance.Mesh = mesh;
                instance.Instance = behaviour;

                foreach (var material in mesh.Materials)
                {
                    List<MeshInstance> phaseList;
                    if (!_phases.TryGetValue(material.Key, out phaseList))
                    {
                        phaseList = new List<MeshInstance>();
                        _phases.Add(material.Key, phaseList);
                    }

                    phaseList.Add(instance);
                }
            }

            private void MeshesAdded(ModelInstance modelInstance, IEnumerable<Mesh> added)
            {
                foreach (var mesh in added)
                    AddMesh(modelInstance, mesh);
            }

            private void RemoveMesh(ModelInstance behaviour, Mesh mesh)
            {
                foreach (var phase in mesh.Materials.Keys)
                {
                    //Get the phase list which contains this mesh
                    List<MeshInstance> phaseList;
                    if (!_phases.TryGetValue(phase, out phaseList))
                        continue;

                    //Remove all instances of this behaviour+mesh
                    phaseList.RemoveAll(a => a.Mesh == mesh && a.Instance == behaviour);
                }
            }

            private void MeshesRemoved(ModelInstance modelInstance, IEnumerable<Mesh> removed)
            {
                foreach (var mesh in removed)
                    RemoveMesh(modelInstance, mesh);
            }

            public override bool Remove(ModelInstance behaviour)
            {
                if (behaviour.Model != null)
                    MeshesRemoved(behaviour, behaviour.Model.Meshes);
                behaviour.ModelDataChanged -= Changed;
                behaviour.ModelMeshAdded -= AddMesh;
                behaviour.ModelMeshRemoved -= RemoveMesh;

                return base.Remove(behaviour);
            }
            #endregion

            public void Query(string phase, NamedBoxCollection metadata, ICollection<IGeometry> result)
            {
                //Get all meshes which want to be drawn in this phase (early exit if there are none)
                List<MeshInstance> meshes;
                if (!_phases.TryGetValue(phase, out meshes))
                    return;

                //Find all items which are in the view bounds
                var viewFrustum = metadata.GetValue(new TypedName<BoundingFrustum>("viewfrustum"));
                _bounds.Clear();
                _bounds.Add(viewFrustum);
                QueryVisible(meshes, _bounds, _buffer);

                //Copy visible instances into the result set
                foreach (var item in _buffer)
                    if (!item.Instance.IsInvisible)
                        result.Add(item);

                //Calculate world view matrices for each mesh
                var view = metadata.GetValue(new TypedName<Matrix4x4>("view"));

                //Calculate WorldView for all mesh instances
                CalculateWorldViews(_buffer, view);

                //Clear the temp query buffer
                _buffer.Clear();
            }

            private static void CalculateWorldViews(IEnumerable<MeshInstance> instances, Matrix4x4 cameraView)
            {
                foreach (var instance in instances)
                {
                    instance.World = instance.Mesh.MeshTransform * instance.Instance.Transform;
                    if (instance.Instance._customViewMatrix.Value.HasValue)
                        instance.WorldView = instance.World * instance.Instance._customViewMatrix.Value.Value;
                    else
                        instance.WorldView = Matrix4x4.Multiply(instance.World, cameraView);
                }
            }

            private static void QueryVisible(IEnumerable<MeshInstance> instances, BoundingVolume volume, ICollection<MeshInstance> meshInstances)
            {
                foreach (var instance in instances)
                {
                    instance.UpdateBounds();

                    //If this item ignores the view and projection matrices all bets are off. Just pass it and let the graphics device deal with it
                    if (instance.Instance._customViewMatrix.Value.HasValue || instance.Instance._customProjectionMatrix.Value.HasValue || volume.Intersects(instance.BoundingSphere))
                        meshInstances.Add(instance);
                }
            }
        }

        public interface IRenderDataSupplier
        {
            void SetRenderData(NamedBoxCollection metadata);

            string Name { get; }
        }
    }
}
