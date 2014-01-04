using System.Collections.Generic;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;

namespace Myre.Graphics.Pipeline.Materials
{
    //[ContentSerializerRuntimeType("Myre.Graphics.Materials.Material, Myre.Graphics")]
    public class MyreMaterialContent
    {
        public Dictionary<string, string> Textures = new Dictionary<string, string>();
        public Dictionary<string, object> OpaqueData = new Dictionary<string, object>();

        public string EffectName;
        public string Technique;
    }

    [ContentTypeWriter]
    public class MyreMaterialWriter : ContentTypeWriter<MyreMaterialContent>
    {
        protected override void Write(ContentWriter output, MyreMaterialContent value)
        {
            output.Write(value.Technique ?? "");
            output.Write(value.EffectName ?? "");

            output.Write(value.Textures.Count);
            foreach (var kvp in value.Textures)
            {
                output.Write(kvp.Key);
                output.Write(kvp.Value);
            }

            output.Write(value.OpaqueData.Count);
            foreach (var kvp in value.OpaqueData)
            {
                output.Write(kvp.Key);
                output.WriteObject(kvp.Value);
            }
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
