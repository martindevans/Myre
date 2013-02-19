using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Content;

namespace Myre.Graphics.Geometry
{
    public interface IModelData
        :IDisposable
    {
        IEnumerable<Mesh> Meshes { get; }

        event Action<IModelData, IEnumerable<Mesh>> MeshesAdded;
        event Action<IModelData, IEnumerable<Mesh>> MeshesRemoved;
    }

    public sealed class ModelData
        :IModelData
    {
        private readonly Mesh[] _meshes;
        public IEnumerable<Mesh> Meshes
        {
            get { return _meshes; }
        }

        public event Action<IModelData, IEnumerable<Mesh>> MeshesAdded;
        public event Action<IModelData, IEnumerable<Mesh>> MeshesRemoved;

        public ModelData(Mesh[] meshes)
        {
            _meshes = meshes;
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
                    for (int i = 0; i < _meshes.Length; i++)
                    {
                        _meshes[i].Dispose();
                        _meshes[i] = null;
                    }
                }

                MeshesAdded = null;
                MeshesRemoved = null;
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