using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Myre.Collections;
using Myre.Debugging.Statistics;
using Myre.Entities;
using Myre.Entities.Behaviours;
using Myre.Graphics.Materials;

namespace Myre.Graphics.Geometry
{
    [DefaultManager(typeof(Manager))]
    public class ModelInstance
        : Behaviour
    {
        private Property<ModelData> model;
        private Property<Matrix> transform;
        private Property<bool> isStatic;
        private Property<bool> isInvisible;

        public ModelData Model
        {
            get { return model.Value; }
            set { model.Value = value; }
        }

        public Matrix Transform
        {
            get { return transform.Value; }
            set { transform.Value = value; }
        }

        public bool IsStatic
        {
            get { return isStatic.Value; }
            set { isStatic.Value = value; }
        }

        public bool IsInvisible
        {
            get { return isInvisible.Value; }
            set { isInvisible.Value = value; }
        }

        public override void CreateProperties(Entity.ConstructionContext context)
        {
            var append = (Name == null ? "" : "_" + Name);

            model = context.CreateProperty<ModelData>("model" + append);
            transform = context.CreateProperty<Matrix>("transform" + append);
            isStatic = context.CreateProperty<bool>("is_static" + append);
            isInvisible = context.CreateProperty<bool>("is_invisible" + append);

            base.CreateProperties(context);
        }


        public class Manager
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
            private static readonly Statistic polysDrawnStat = Statistic.Get("Graphics.Primitives");
            private static readonly Statistic drawsStat = Statistic.Get("Graphics.Draws");
#endif

            private readonly GraphicsDevice device;
            private readonly Pool<MeshInstance> meshInstancePool;
            private readonly Dictionary<Mesh, List<MeshInstance>> instances;
            private readonly Dictionary<string, List<MeshRenderData>> phases;
            private readonly List<MeshInstance> dynamicMeshInstances;
            private List<MeshInstance> staticMeshInstances;
            private readonly List<MeshInstance> buffer;
            private readonly List<MeshInstance> visibleInstances;
            private readonly List<ICullable> cullableBuffer;
            private readonly BoundingVolume bounds;
            private readonly Octree octree;

            public Manager(
                GraphicsDevice device)
            {
                this.device = device;
                meshInstancePool = new Pool<MeshInstance>();
                instances = new Dictionary<Mesh, List<MeshInstance>>();
                phases = new Dictionary<string, List<MeshRenderData>>();
                dynamicMeshInstances = new List<MeshInstance>();
                staticMeshInstances = new List<MeshInstance>();
                buffer = new List<MeshInstance>();
                visibleInstances = new List<MeshInstance>();
                cullableBuffer = new List<ICullable>();
                bounds = new BoundingVolume();
                octree = new Octree();
            }

            public override void Add(ModelInstance behaviour)
            {
                foreach (var mesh in behaviour.Model.Meshes)
                {
                    var instance = meshInstancePool.Get();
                    instance.Mesh = mesh;
                    instance.Instance = behaviour;
                    instance.IsVisible = false;
                    
                    GetInstanceList(mesh).Add(instance);
                    dynamicMeshInstances.Add(instance);
                    //instance.UpdateBounds();
                    //octree.Add(instance);
                }

                base.Add(behaviour);
            }

            public override bool Remove(ModelInstance behaviour)
            {
                foreach (var mesh in behaviour.Model.Meshes)
                {
                    var instances = GetInstanceList(mesh);
                    for (int i = 0; i < instances.Count; i++)
                    {
                        if (instances[i].Instance == behaviour)
                        {
                            dynamicMeshInstances.Remove(instances[i]);
                            //octree.Remove(instances[i]);
                            meshInstancePool.Return(instances[i]);
                            instances.RemoveAt(i);
                            break;
                        }
                    }
                }

                return base.Remove(behaviour);
            }

            public void Draw(string phase, BoxedValueStore<string> metadata)
            {
                List<MeshRenderData> meshes;
                if (!phases.TryGetValue(phase, out meshes))
                    return;

                var viewFrustum = metadata.Get<BoundingFrustum>("viewfrustum").Value;
                bounds.Clear();
                bounds.Add(viewFrustum);
                QueryVisible(bounds, buffer);

                foreach (var item in buffer)
                    item.IsVisible = true & !item.Instance.IsInvisible;

                var view = metadata.Get<Matrix>("view");
                CalculateWorldViews(meshes, ref view.Value);    //Calculate WorldView for all mesh instances

                DepthSortMeshes(meshes);                        //Sort batches by first item in batch

                foreach (var mesh in meshes)
                {
                    foreach (var instance in mesh.Instances)
                    {
                        if (instance.IsVisible)
                            visibleInstances.Add(instance);
                    }

                    if (visibleInstances.Count > 0)
                    {
                        DrawMesh(mesh, metadata);
                        visibleInstances.Clear();
                    }
                }

                foreach (var item in buffer)
                    item.IsVisible = false;
                buffer.Clear();
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

            private void CalculateWorldViews(List<MeshRenderData> batches, ref Matrix view)
            {
                for (int b = 0; b < batches.Count; b++)
                {
                    var instances = batches[b].Instances;
                    for (int i = 0; i < instances.Count; i++)
                    {
                        var instance = instances[i];
                        Matrix world = instance.Instance.Transform;
                        Matrix.Multiply(ref world, ref view, out instance.WorldView);
                    }
                }
            }

            private void QueryVisible(BoundingVolume volume, List<MeshInstance> instances)
            {
                cullableBuffer.Clear();
                octree.Query(cullableBuffer, volume);
                foreach (var item in cullableBuffer)
                {
                    var instance = (MeshInstance)item;
                    instance.UpdateBounds();
                    if (volume.Intersects(instance.Bounds))
                        instances.Add(item as MeshInstance);
                }

                foreach (var item in dynamicMeshInstances)
                {
                    item.UpdateBounds();
                    if (volume.Intersects(item.Bounds))
                        instances.Add(item);
                }
            }

            private void DrawMesh(MeshRenderData data, BoxedValueStore<string> metadata)
            {
                var mesh = data.Mesh;
                device.SetVertexBuffer(mesh.VertexBuffer);
                device.Indices = mesh.IndexBuffer;

                var world = metadata.Get<Matrix>("world");
                var projection = metadata.Get<Matrix>("projection");
                var worldView = metadata.Get<Matrix>("worldview");
                var worldViewProjection = metadata.Get<Matrix>("worldviewprojection");

                DepthSortInstances(visibleInstances);

                int maxPrimitives = device.GraphicsProfile == GraphicsProfile.HiDef ? 1048575 : 65535;

                for (int i = 0; i < visibleInstances.Count; i++)
                {
                    var instance = visibleInstances[i];

                    world.Value = instance.Instance.Transform;
                    worldView.Value = instance.WorldView;
                    Matrix.Multiply(ref worldView.Value, ref projection.Value, out worldViewProjection.Value);

                    foreach (var pass in data.Material.Begin(metadata))
                    {
                        //Loop through mesh, drawing as many primitives as possible per batch
                        if (mesh.TriangleCount == 0 || mesh.VertexCount == 0)
                            continue;

                        pass.Apply();

                        int primitives = mesh.TriangleCount;
                        int offset = 0;
                        while (primitives > 0)
                        {
                            int primitiveCount = Math.Min(primitives, maxPrimitives);
                            primitives -= primitiveCount;

                            device.DrawIndexedPrimitives(PrimitiveType.TriangleList, mesh.BaseVertex, mesh.MinVertexIndex, mesh.VertexCount, mesh.StartIndex + offset * 3, primitiveCount);

                            offset += primitiveCount;
                        }

#if PROFILE
                        polysDrawnStat.Value += mesh.TriangleCount;
                        drawsStat.Value++;
#endif
                    }
                }
            }

            private void DepthSortInstances(List<MeshInstance> instances)
            {
                if (instances.Count > 1)
                    instances.Sort(InstanceComparator);
            }

            private int InstanceComparator(MeshInstance a, MeshInstance b)
            {
                return CompareWorldViews(ref a.WorldView, ref b.WorldView);
            }

            public static int CompareWorldViews(ref Matrix worldViewA, ref Matrix worldViewB)
            {
                //Negated, because XNA uses a negative Z space
                return -worldViewA.Translation.Z.CompareTo(worldViewB.Translation.Z);
            }

            private List<MeshInstance> GetInstanceList(Mesh mesh)
            {
                List<MeshInstance> value;
                if (!instances.TryGetValue(mesh, out value))
                {
                    value = new List<MeshInstance>();
                    instances[mesh] = value;

                    RegisterMeshParts(mesh, value);
                }

                return value;
            }

            private void RegisterMeshParts(Mesh mesh, List<MeshInstance> instances)
            {
                foreach (var material in mesh.Materials)
                {
                    List<MeshRenderData> data;
                    if (!phases.TryGetValue(material.Key, out data))
                    {
                        data = new List<MeshRenderData>();
                        phases[material.Key] = data;
                    }

                    data.Add(new MeshRenderData()
                    {
                        Mesh = mesh,
                        Material = material.Value,
                        Instances = instances
                    });
                }
            }
        }
    }
}
