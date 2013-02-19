using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Content;

namespace Myre.Graphics.Geometry
{
    public sealed class ModelData
        :IDisposable
    {
        private readonly List<Mesh> _meshes;
        public IEnumerable<Mesh> Meshes
        {
            get { return _meshes; }
        }

        public event Action<ModelData, Mesh> MeshAdded;
        public event Action<ModelData, Mesh> MeshRemoved;

        public ModelData(IEnumerable<Mesh> meshes)
        {
            _meshes = new List<Mesh>(meshes);
        }

        public void Add(Mesh m)
        {
            _meshes.Add(m);
            MeshAdded(this, m);
        }

        public bool Remove(Mesh m)
        {
            bool removed = _meshes.Remove(m);
            if (removed)
                MeshRemoved(this, m);
            return removed;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private bool _disposed;

        private void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
            {
                if (Meshes != null)
                {
                    for (int i = 0; i < _meshes.Count; i++)
                    {
                        _meshes[i].Dispose();
                        _meshes[i] = null;
                    }
                }

                MeshAdded = null;
                MeshRemoved = null;
            }

            _disposed = true;
        }

        ~ModelData()
        {
            Dispose(false);
        }
    }

    public class ModelReader : ContentTypeReader<ModelData>
    {
        protected override ModelData Read(ContentReader input, ModelData existingInstance)
        {
            var size = input.ReadInt32();
            var meshes = new Mesh[size];
            for (int i = 0; i < size; i++)
                meshes[i] = input.ReadObject<Mesh>();

            var model = existingInstance ?? new ModelData(meshes);

            return model;
        }
    }
}