using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;

namespace Myre.Graphics.Pipeline
{
    [ContentSerializerRuntimeType("Myre.Graphics.Geometry.Mesh, Myre.Graphics")]
    public class MyreMeshContent
    {
        public string Name { get; set; }
        public BoundingSphere BoundingSphere { get; set; }

        public Dictionary<string, MyreMaterialContent> Materials { get; set; }
        public int TriangleCount { get; set; }
        public int VertexCount { get; set; }
        public VertexBufferContent VertexBuffer { get; set; }
        public IndexCollection IndexBuffer { get; set; }
    }

    [ContentTypeWriter]
    public class MeshContentWriter : ContentTypeWriter<MyreMeshContent>
    {
        protected override void Write(ContentWriter output, MyreMeshContent value)
        {
            //output.WriteObject(value.Parent);

            //output.Write(value.Parts.Length);
            //foreach (var item in value.Parts)
            //{
            //    output.WriteObject(item);
            //}

            output.Write(value.Name);
            output.Write(value.VertexCount);
            output.Write(value.TriangleCount);
            output.WriteObject(value.VertexBuffer);
            output.WriteObject(value.IndexBuffer);

            // manually write out the dictionary, as the dictionary reader class DOES NOT EXIST
            output.Write(value.Materials.Count);
            foreach (var item in value.Materials)
            {
                output.WriteObject(item.Key);
                output.WriteSharedResource(item.Value);
            }

            output.WriteObject(value.BoundingSphere);
        }

        public override string GetRuntimeReader(TargetPlatform targetPlatform)
        {
            return "Myre.Graphics.Geometry.MeshReader, Myre.Graphics";
        }

        public override string GetRuntimeType(TargetPlatform targetPlatform)
        {
            return "Myre.Graphics.Geometry.Mesh, Myre.Graphics";
        }
    }
}
