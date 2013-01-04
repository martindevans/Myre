using System;
using Microsoft.Xna.Framework.Content;

namespace Myre.Graphics.Geometry
{
    public sealed class ModelData
        :IDisposable
    {
        public Mesh[] Meshes { get; set; }
        //public ModelBone[] Skeleton { get; set; }

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
                    if (Meshes != null)
                        foreach (var mesh in Meshes)
                            mesh.Dispose();
                    Meshes = null;
                }

                _disposed = true;
            }
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
            var model = existingInstance ?? new ModelData();

            var size = input.ReadInt32();
            model.Meshes = new Mesh[size];
            for (int i = 0; i < size; i++)
                model.Meshes[i] = input.ReadObject<Mesh>();

            //size = input.ReadInt32();
            //model.Skeleton = new ModelBone[size];
            //for (int i = 0; i < size; i++)
            //    model.Skeleton[i] = input.ReadObject<ModelBone>();

            return model;
        }
    }
}