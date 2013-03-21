using System.Collections.Generic;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;

namespace Myre.Graphics.Pipeline
{
    [ContentSerializerRuntimeType("Myre.Graphics.Geometry.ModelData, Myre.Graphics")]
    public class MyreModelContent
    {
        private readonly List<MyreMeshContent> _meshes;

        public MyreMeshContent[] Meshes { get { return _meshes.ToArray(); } }
        //public BoneContent[] Skeleton { get; set; }

        public MyreModelContent()
        {
            _meshes = new List<MyreMeshContent>();
        }

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
            output.Write(value.Meshes.Length);
            foreach (var item in value.Meshes)
            {
                output.WriteObject(item);
            }

            //output.Write(value.Skeleton.Length);
            //foreach (var item in value.Skeleton)
            //{
            //    output.WriteObject(item);
            //}
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
