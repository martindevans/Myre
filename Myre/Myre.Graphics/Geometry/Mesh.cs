using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Myre.Graphics.Materials;

namespace Myre.Graphics.Geometry
{
    public sealed class Mesh
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
        public Matrix MeshTransform { get; set; }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private bool _disposed;

        private void Dispose(bool disposing)
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

            mesh.MeshTransform = Matrix.Identity;

            mesh.Name = input.ReadString();
            mesh.VertexCount = input.ReadInt32();
            mesh.TriangleCount = input.ReadInt32();

            bool hasVertexData = input.ReadBoolean();
            if (hasVertexData)
                mesh.VertexBuffer = input.ReadObject<VertexBuffer>();

            bool hasIndexData = input.ReadBoolean();
            if (hasIndexData)
                mesh.IndexBuffer = input.ReadObject<IndexBuffer>();

            mesh.StartIndex = input.ReadInt32();
            mesh.BaseVertex = input.ReadInt32();
            mesh.MinVertexIndex = input.ReadInt32();

            var size = input.ReadInt32();
            mesh.Materials = new Dictionary<string, Material>(size);
            for (int i = 0; i < size; i++)
            {
                var key = input.ReadString();
                var material = input.ReadObject<Material>();
                mesh.Materials.Add(key, material);
            }

            mesh.BoundingSphere = input.ReadObject<BoundingSphere>();

            return mesh;
        }
    }
}
