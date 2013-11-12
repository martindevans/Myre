using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Myre.Collections;
using Myre.Debugging.Statistics;
using Myre.Entities;
using Myre.Entities.Behaviours;
using Myre.Graphics.Materials;
using System.Linq;

namespace Myre.Graphics.Geometry
{
    [DefaultManager(typeof(Manager))]
    public class ModelInstance
        : Behaviour
    {
        private Property<ModelData> _model;
        private Property<Matrix> _transform;
        private Property<bool> _isStatic;
        private Property<bool> _isInvisible;
        private Property<bool> _ignoreViewMatrix;
        private Property<bool> _ignoreProjectionMatrix;

        private IRenderDataSupplier[] _renderDataSuppliers;

        public ModelData Model
        {
            get { return _model.Value; }
            set { _model.Value = value; }
        }

        public Matrix Transform
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
            var append = (Name == null ? "" : "_" + Name);

            _model = context.CreateProperty<ModelData>("model" + append);
            _transform = context.CreateProperty<Matrix>("transform" + append);
            _isStatic = context.CreateProperty<bool>("is_static" + append);
            _isInvisible = context.CreateProperty<bool>("is_invisible" + append);
            _ignoreViewMatrix = context.CreateProperty<bool>("ignore_view_matrix" + append, false);
            _ignoreProjectionMatrix = context.CreateProperty<bool>("ignore_projection_matrix" + append, false);

            base.CreateProperties(context);
        }

        public override void Initialise(INamedDataProvider initialisationData)
        {
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
                : ICullable
            {
                public Mesh Mesh;
                public ModelInstance Instance;
                public bool IsVisible;

                public BoundingSphere Bounds { get; private set; }

                public Matrix WorldView;

                public void UpdateBounds()
                {
                    Bounds = Mesh.BoundingSphere.Transform(Instance.Transform);
                }
            }

            class MeshRenderData
            {
                public Mesh Mesh;
                public Material Material;
                public List<MeshInstance> Instances;
            }

#if PROFILE
            private static readonly Statistic _polysDrawnStat = Statistic.Get("Graphics.Primitives");
            private static readonly Statistic _drawsStat = Statistic.Get("Graphics.Draws");
#endif

            private readonly GraphicsDevice _device;
            private readonly Pool<MeshInstance> _meshInstancePool;
            private readonly Dictionary<Mesh, List<MeshInstance>> _instances;
            private readonly Dictionary<string, List<MeshRenderData>> _phases;
            private readonly List<MeshInstance> _dynamicMeshInstances;
            private readonly List<MeshInstance> _buffer;
            private readonly List<MeshInstance> _visibleInstances;
            private readonly BoundingVolume _bounds;

            public Manager(
                GraphicsDevice device)
            {
                _device = device;
                _meshInstancePool = new Pool<MeshInstance>();
                _instances = new Dictionary<Mesh, List<MeshInstance>>();
                _phases = new Dictionary<string, List<MeshRenderData>>();
                _dynamicMeshInstances = new List<MeshInstance>();
                _buffer = new List<MeshInstance>();
                _visibleInstances = new List<MeshInstance>();
                _bounds = new BoundingVolume();
            }

            public override void Add(ModelInstance behaviour)
            {
                if (behaviour.Model != null)
                    MeshesAdded(behaviour, behaviour.Model.Meshes);
                behaviour.ModelDataChanged += Changed;
                behaviour.ModelMeshAdded += AddMesh;
                behaviour.ModelMeshAdded += RemoveMesh;

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
                instance.IsVisible = false;

                GetInstanceList(mesh).Add(instance);
                _dynamicMeshInstances.Add(instance);
            }

            private void MeshesAdded(ModelInstance modelInstance, IEnumerable<Mesh> added)
            {
                foreach (var mesh in added)
                    AddMesh(modelInstance, mesh);
            }

            private void RemoveMesh(ModelInstance behaviour, Mesh mesh)
            {
                var meshInstances = GetInstanceList(mesh);
                for (int i = 0; i < meshInstances.Count; i++)
                {
                    if (meshInstances[i].Instance == behaviour)
                    {
                        _dynamicMeshInstances.Remove(meshInstances[i]);
                        _meshInstancePool.Return(meshInstances[i]);
                        meshInstances.RemoveAt(i);
                        break;
                    }
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

            public void Draw(string phase, NamedBoxCollection metadata)
            {
                List<MeshRenderData> meshes;
                if (!_phases.TryGetValue(phase, out meshes))
                    return;

                var viewFrustum = metadata.GetValue<BoundingFrustum>("viewfrustum");
                _bounds.Clear();
                _bounds.Add(viewFrustum);
                QueryVisible(_bounds, _buffer);

                foreach (var item in _buffer)
                    item.IsVisible = !item.Instance.IsInvisible;

                var view = metadata.GetValue<Matrix>("view");
                CalculateWorldViews(meshes, view);              //Calculate WorldView for all mesh instances

                DepthSortMeshes(meshes);                        //Sort batches by first item in batch

                foreach (var mesh in meshes)
                {
                    foreach (var instance in mesh.Instances)
                    {
                        if (instance.IsVisible)
                            _visibleInstances.Add(instance);
                    }

                    if (_visibleInstances.Count > 0)
                    {
                        DrawMesh(mesh, metadata);
                        _visibleInstances.Clear();
                    }
                }

                foreach (var item in _buffer)
                    item.IsVisible = false;
                _buffer.Clear();
            }

            private void DepthSortMeshes(List<MeshRenderData> meshes)
            {
                meshes.Sort(RenderDataComparator);
            }

            private int RenderDataComparator(MeshRenderData a, MeshRenderData b)
            {
                if (a.Instances.Count > 0 && b.Instances.Count > 0)
                    return CompareWorldViews(ref a.Instances[0].WorldView, ref b.Instances[0].WorldView);
                return a.Instances.Count.CompareTo(b.Instances.Count);
            }

            private void CalculateWorldViews(List<MeshRenderData> batches, Matrix cameraView)
            {
                for (int b = 0; b < batches.Count; b++)
                {
                    var meshInstances = batches[b].Instances;
                    for (int i = 0; i < meshInstances.Count; i++)
                    {
                        var instance = meshInstances[i];
                        Matrix world = instance.Instance.Transform;
                        if (instance.Instance._ignoreViewMatrix.Value)
                            instance.WorldView = world;
                        else
                            Matrix.Multiply(ref world, ref cameraView, out instance.WorldView);
                    }
                }
            }

            private void QueryVisible(BoundingVolume volume, List<MeshInstance> meshInstances)
            {
                foreach (var item in _dynamicMeshInstances)
                {
                    item.UpdateBounds();
                    //If this item ignores the view and projection matrices all bets are off. Just pass it and let the graphics device deal with it
                    if (item.Instance._ignoreViewMatrix.Value || item.Instance._ignoreProjectionMatrix.Value || volume.Intersects(item.Bounds))
                        meshInstances.Add(item);
                }
            }

            private void DrawMesh(MeshRenderData data, NamedBoxCollection metadata)
            {
                var mesh = data.Mesh;
                _device.SetVertexBuffer(mesh.VertexBuffer);
                _device.Indices = mesh.IndexBuffer;

                var world = metadata.Get<Matrix>("world", default(Matrix), true);
                var projection = metadata.Get<Matrix>("projection", default(Matrix), true);
                var worldView = metadata.Get<Matrix>("worldview", default(Matrix), true);
                var worldViewProjection = metadata.Get<Matrix>("worldviewprojection", default(Matrix), true);

                DepthSortInstances(_visibleInstances);

                int maxPrimitives = _device.GraphicsProfile == GraphicsProfile.HiDef ? 1048575 : 65535;

                for (int i = 0; i < _visibleInstances.Count; i++)
                {
                    var instance = _visibleInstances[i];

                    instance.Instance.ApplyRendererData(metadata);

                    world.Value = instance.Instance.Transform;
                    worldView.Value = instance.WorldView;
                    if (instance.Instance._ignoreProjectionMatrix.Value)
                        worldViewProjection.Value = worldView.Value;
                    else
                        worldViewProjection.Value = worldView.Value * projection.Value;

                    foreach (var pass in data.Material.Begin(metadata))
                    {
                        //Skip this if it has no vertices or triangles
                        if (mesh.TriangleCount == 0 || mesh.VertexCount == 0)
                            continue;

                        pass.Apply();

                        //Loop through mesh, drawing as many primitives as possible per batch
                        int primitives = mesh.TriangleCount;
                        int offset = 0;
                        while (primitives > 0)
                        {
                            int primitiveCount = Math.Min(primitives, maxPrimitives);
                            primitives -= primitiveCount;

                            _device.DrawIndexedPrimitives(PrimitiveType.TriangleList, mesh.BaseVertex, mesh.MinVertexIndex, mesh.VertexCount, mesh.StartIndex + offset * 3, primitiveCount);

                            offset += primitiveCount;
                        }

#if PROFILE
                        _polysDrawnStat.Value += mesh.TriangleCount;
                        _drawsStat.Value++;
#endif
                    }
                }
            }

            private void DepthSortInstances(List<MeshInstance> meshInstances)
            {
                if (meshInstances.Count > 1)
                    meshInstances.Sort(InstanceComparator);
            }

            private static int InstanceComparator(MeshInstance a, MeshInstance b)
            {
                return CompareWorldViews(ref a.WorldView, ref b.WorldView);
            }

            private static int CompareWorldViews(ref Matrix worldViewA, ref Matrix worldViewB)
            {
                //Negated, because XNA uses a negative Z space
                return -worldViewA.Translation.Z.CompareTo(worldViewB.Translation.Z);
            }

            private List<MeshInstance> GetInstanceList(Mesh mesh)
            {
                List<MeshInstance> value;
                if (!_instances.TryGetValue(mesh, out value))
                {
                    value = new List<MeshInstance>();
                    _instances[mesh] = value;

                    RegisterMeshParts(mesh, value);
                }

                return value;
            }

            private void RegisterMeshParts(Mesh mesh, List<MeshInstance> meshInstances)
            {
                foreach (var material in mesh.Materials)
                {
                    List<MeshRenderData> data;
                    if (!_phases.TryGetValue(material.Key, out data))
                    {
                        data = new List<MeshRenderData>();
                        _phases[material.Key] = data;
                    }

                    data.Add(new MeshRenderData()
                    {
                        Mesh = mesh,
                        Material = material.Value,
                        Instances = meshInstances
                    });
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
