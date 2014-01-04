using System.Collections.Generic;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;
using Myre.Graphics.Pipeline.Animations;

namespace Myre.Graphics.Pipeline.Models
{
    [ContentSerializerRuntimeType("Myre.Graphics.Geometry.ModelData, Myre.Graphics")]
    public class MyreModelContent
    {
        private readonly List<MyreMeshContent> _meshes = new List<MyreMeshContent>();
        public MyreMeshContent[] Meshes { get { return _meshes.ToArray(); } }

        public SkinningDataContent SkinningData { get; set; }

        internal void AddMesh(MyreMeshContent mesh)
        {
            _meshes.Add(mesh);
        }
    }

    [ContentTypeWriter]
    public class MyreModelContentWriter : ContentTypeWriter<MyreModelContent>
    {
        protected override void Write(ContentWriter output, MyreModelContent value)
        {
            //Write out meshes
            output.Write(value.Meshes.Length);
            foreach (var item in value.Meshes)
            {
                output.WriteObject(item);
            }

            //Write out animation data
            output.Write(value.SkinningData != null);
            if (value.SkinningData != null)
                output.WriteObject(value.SkinningData);
        }

        public override string GetRuntimeReader(TargetPlatform targetPlatform)
        {
            return "Myre.Graphics.Geometry.ModelReader, Myre.Graphics";
        }

        public override string GetRuntimeType(TargetPlatform targetPlatform)
        {
            return "Myre.Graphics.Geometry.ModelData, Myre.Graphics";
        }
    }
}
