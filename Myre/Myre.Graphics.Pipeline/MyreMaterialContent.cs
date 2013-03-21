using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;

namespace Myre.Graphics.Pipeline
{
    //[ContentSerializerRuntimeType("Myre.Graphics.Materials.Material, Myre.Graphics")]
    public class MyreMaterialContent
    {
        //[ContentSerializer(SharedResource=true)]
        public MaterialContent Material;
        public string Technique;
    }

    [ContentTypeWriter]
    public class MyreMaterialWriter : ContentTypeWriter<MyreMaterialContent>
    {
        protected override void Write(ContentWriter output, MyreMaterialContent value)
        {
            output.Write(value.Technique ?? "");
            output.WriteObject(value.Material);
        }

        public override string GetRuntimeReader(TargetPlatform targetPlatform)
        {
            return "Myre.Graphics.Materials.MaterialReader, Myre.Graphics";
        }

        public override string GetRuntimeType(TargetPlatform targetPlatform)
        {
            return "Myre.Graphics.Materials.Material, Myre.Graphics";
        }
    }
}
