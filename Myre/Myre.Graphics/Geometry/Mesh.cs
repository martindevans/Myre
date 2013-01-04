using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Myre.Graphics.Materials;

namespace Myre.Graphics.Geometry
{
    public class Mesh
        :IDisposable
    {
        public string Name { get; set; }
        public int TriangleCount { get; set; }
        public int VertexCount { get; set; }
        public VertexBuffer VertexBuffer { get; set; }
        public IndexBuffer IndexBuffer { get; set; }
        public Dictionary<string, Material> Materials { get; set; }
        public BoundingSphere BoundingSphere { get; set; }
        public int StartIndex { get; set; }
        public int BaseVertex { get; set; }
        public int MinVertexIndex { get; set; }
        //public ModelBone ParentBone { get; set; }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private bool _disposed;
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    if (VertexBuffer != null)
                        VertexBuffer.Dispose();
                    VertexBuffer = null;

                    if (IndexBuffer != null)
                        IndexBuffer.Dispose();
                    IndexBuffer = null;
                }

                _disposed = true;
            }
        }

        ~Mesh()
        {
            Dispose(false);
        }
    }

    public class MeshReader : ContentTypeReader<Mesh>
    {
        protected override Mesh Read(ContentReader input, Mesh existingInstance)
        {
            var mesh = existingInstance ?? new Mesh();

            //mesh.ParentBone = input.ReadObject<ModelBone>();

            //var size = input.ReadInt32();
            //mesh.Parts = new MeshPart[size];
            //for (int i = 0; i < size; i++)
            //    mesh.Parts[i] = input.ReadObject<MeshPart>();

            mesh.Name = input.ReadString();
            mesh.VertexCount = input.ReadInt32();
            mesh.TriangleCount = input.ReadInt32();
            mesh.VertexBuffer = input.ReadObject<VertexBuffer>();
            mesh.IndexBuffer = input.ReadObject<IndexBuffer>();

            var size = input.ReadInt32();
            mesh.Materials = new Dictionary<string, Materials.Material>(size);
            for (int i = 0; i < size; i++)
            {
                var key = input.ReadObject<string>();
                input.ReadSharedResource<Material>(m => mesh.Materials.Add(key, m));
            }

            mesh.BoundingSphere = input.ReadObject<BoundingSphere>();

            return mesh;
        }
    }
}
