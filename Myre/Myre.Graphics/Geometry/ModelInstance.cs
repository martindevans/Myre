using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Myre.Entities.Behaviours;
using Myre.Entities;
using Myre.Collections;
using Microsoft.Xna.Framework;
using Myre.Graphics.Materials;
using Microsoft.Xna.Framework.Graphics;
using Myre.Debugging.Statistics;

namespace Myre.Graphics.Geometry
{
    [DefaultManager(typeof(Manager))]
    public class ModelInstance
        : Behaviour
    {
        private Property<ModelData> model;
        private Property<Matrix> transform;
        private Property<bool> isStatic;

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

        public override void CreateProperties(Entity.ConstructionContext context)
        {
            this.model = context.CreateProperty<ModelData>("model");
            this.transform = context.CreateProperty<Matrix>("transform");
            this.isStatic = context.CreateProperty<bool>("is_static");

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
            private static Statistic polysDrawnStat = Statistic.Get("Graphics.Primitives");
            private static Statistic drawsStat = Statistic.Get("Graphics.Draws");
#endif

            private GraphicsDevice device;
            private Pool<MeshInstance> meshInstancePool;
            private Dictionary<Mesh, List<MeshInstance>> instances;
            private Dictionary<string, List<MeshRenderData>> phases;
            private List<MeshInstance> dynamicMeshInstances;
            private List<MeshInstance> staticMeshInstances;
            private List<MeshInstance> buffer;
            private List<MeshInstance> visibleInstances;
            private List<ICullable> cullableBuffer;
            private BoundingVolume bounds;
            private Octree octree;

            public Manager(
                GraphicsDevice device)
            {
                this.device = device;
                this.meshInstancePool = new Pool<MeshInstance>();
                this.instances = new Dictionary<Mesh, List<MeshInstance>>();
                this.phases = new Dictionary<string, List<MeshRenderData>>();
                this.dynamicMeshInstances = new List<MeshInstance>();
                this.staticMeshInstances = new List<MeshInstance>();
                this.buffer = new List<MeshInstance>();
                this.visibleInstances = new List<MeshInstance>();
                this.cullableBuffer = new List<ICullable>();
                this.bounds = new BoundingVolume();
                this.octree = new Octree();
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
                    item.IsVisible = true;

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

            public void Query(IList<Entity> results, BoundingVolume volume, bool detailedCheck = false)
            {
                buffer.Clear();
                QueryVisible(volume, buffer, detailedCheck);
                foreach (var item in buffer)
                    results.Add(item.Instance.Owner);
            }

            public void Query(IList<Entity> results, Ray ray, bool detailedCheck = false)
            {
                throw new NotImplementedException();
            }

            public void Query(IList<ICullable> results, BoundingVolume volume, bool detailedCheck = false)
            {
                buffer.Clear();
                QueryVisible(volume, buffer, detailedCheck);
                foreach (var item in buffer)
                    results.Add(item);
            }

            public void Query(IList<ICullable> results, Ray ray, bool detailedCheck = false)
            {
                throw new NotImplementedException();
            }

            private void QueryVisible(BoundingVolume volume, List<MeshInstance> instances, bool detailedCheck = false)
            {
                if (detailedCheck)
                    throw new NotImplementedException();

                cullableBuffer.Clear();
                octree.Query(cullableBuffer, volume);
                foreach (var item in cullableBuffer)
                {
                    var instance = item as MeshInstance;
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
                var view = metadata.Get<Matrix>("view");
                var projection = metadata.Get<Matrix>("projection");
                var worldView = metadata.Get<Matrix>("worldview");
                var worldViewProjection = metadata.Get<Matrix>("worldviewprojection");

                for (int i = 0; i < visibleInstances.Count; i++)
                {
                    var instance = visibleInstances[i];

                    world.Value = instance.Instance.Transform;
                    Matrix.Multiply(ref world.Value, ref view.Value, out worldView.Value);
                    Matrix.Multiply(ref worldView.Value, ref projection.Value, out worldViewProjection.Value);

                    foreach (var pass in data.Material.Begin(metadata))
                    {
                        pass.Apply();

                        //Loop through mesh, drawing as many primitives as possible per batch
                        int maxPrimitives = device.GraphicsProfile == GraphicsProfile.HiDef ? 1048575 : 65535;
                        int primitives = mesh.IndexBuffer.IndexCount / 3;
                        while (primitives > 0)
                        {
                            int primitiveCount = Math.Min(primitives, maxPrimitives);
                            primitives -= primitiveCount;

                            device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, mesh.VertexCount, 0, primitiveCount);
                        }

#if PROFILE
                        polysDrawnStat.Value += mesh.TriangleCount;
                        drawsStat.Value++;
#endif
                    }
                }
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
