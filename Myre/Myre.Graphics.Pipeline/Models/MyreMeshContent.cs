using System.Collections.Generic;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;
using Myre.Graphics.Pipeline.Materials;

namespace Myre.Graphics.Pipeline.Models
{
    [ContentSerializerRuntimeType("Myre.Graphics.Geometry.Mesh, Myre.Graphics")]
    public class MyreMeshContent
    {
        public string Name { get; set; }
        public Microsoft.Xna.Framework.BoundingSphere BoundingSphere { get; set; }

        public Dictionary<string, MyreMaterialContent> Materials { get; set; }
        public int TriangleCount { get; set; }
        public int VertexCount { get; set; }
        public VertexBufferContent VertexBuffer { get; set; }
        public IndexCollection IndexBuffer { get; set; }

        public int StartIndex { get; set; }
        public int BaseVertex { get; set; }
        public int MinVertexIndex { get; set; }

        public ushort? ParentBoneIndex { get; set; }
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

            output.Write(value.ParentBoneIndex.HasValue);
            if (value.ParentBoneIndex.HasValue)
                output.Write(value.ParentBoneIndex.Value);

            bool hasVertexData = value.VertexBuffer.VertexData.Length > 0;
            output.Write(hasVertexData);
            if (hasVertexData)
                output.WriteObject(value.VertexBuffer);

            bool hasIndexData = value.IndexBuffer.Count > 0;
            output.Write(hasIndexData);
            if (hasIndexData)
                output.WriteObject(value.IndexBuffer);

            output.Write(value.StartIndex);
            output.Write(value.BaseVertex);
            output.Write(value.MinVertexIndex);

            // manually write out the dictionary, as the dictionary reader class DOES NOT EXIST
            output.Write(value.Materials.Count);
            foreach (var item in value.Materials)
            {
                output.Write(item.Key);
                output.WriteObject<MyreMaterialContent>(item.Value);
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
