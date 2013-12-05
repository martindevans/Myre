using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Intermediate;
using Microsoft.Xna.Framework.Graphics;

namespace Myre.Graphics.Pipeline.Serializers
{
    [ContentTypeSerializer]
    public class BlendStateSerializer : ContentTypeSerializer<BlendState>
    {
        protected override BlendState Deserialize(IntermediateReader input, ContentSerializerAttribute format, BlendState existingInstance)
        {
            var value = input.Xml.ReadContentAsString().ToLower();
            switch (value)
            {
                case "additive":
                    return BlendState.Additive;
                case "alphablend":
                    return BlendState.AlphaBlend;
                case "nonpremultiplied":
                    return BlendState.NonPremultiplied;
                case "opaque":
                    return BlendState.Opaque;
                default:
                    throw new InvalidContentException("Unknown BlendState: " + value);
            }
        }

        protected override void Serialize(IntermediateWriter output, BlendState value, ContentSerializerAttribute format)
        {
            if (BlendStateEquals(value, BlendState.Additive))
                output.Xml.WriteString("Additive");
            else if (BlendStateEquals(value, BlendState.AlphaBlend))
                output.Xml.WriteString("AlphaBlend");
            else if (BlendStateEquals(value, BlendState.NonPremultiplied))
                output.Xml.WriteString("NonPremultiplied");
            else if (BlendStateEquals(value, BlendState.Opaque))
                output.Xml.WriteString("Opaque");
            else
                throw new InvalidContentException("Unknown BlendState: " + value);
        }

        static bool BlendStateEquals(BlendState a, BlendState b)
        {
            return a.AlphaBlendFunction == b.AlphaBlendFunction &&
                   a.AlphaDestinationBlend == b.AlphaDestinationBlend &&
                   a.AlphaSourceBlend == b.AlphaSourceBlend &&
                   a.BlendFactor == b.BlendFactor &&
                   a.ColorBlendFunction == b.ColorBlendFunction &&
                   a.ColorDestinationBlend == b.ColorDestinationBlend &&
                   a.ColorSourceBlend == b.ColorSourceBlend &&
                   a.ColorWriteChannels == b.ColorWriteChannels &&
                   a.ColorWriteChannels1 == b.ColorWriteChannels1 &&
                   a.ColorWriteChannels2 == b.ColorWriteChannels2 &&
                   a.ColorWriteChannels3 == b.ColorWriteChannels3 &&
                   a.MultiSampleMask == b.MultiSampleMask;
        }
    }

    [ContentTypeWriter]
    public class BlendStateWriter : ContentTypeWriter<BlendState>
    {
        protected override void Write(ContentWriter output, BlendState value)
        {
            output.WriteObject<string>(value.Name);
            output.WriteObject<BlendFunction>(value.AlphaBlendFunction);
            output.WriteObject<Blend>(value.AlphaDestinationBlend);
            output.WriteObject<Blend>(value.AlphaSourceBlend);
            output.WriteObject<Color>(value.BlendFactor);
            output.WriteObject<BlendFunction>(value.ColorBlendFunction);
            output.WriteObject<Blend>(value.ColorDestinationBlend);
            output.WriteObject<Blend>(value.ColorSourceBlend);
            output.WriteObject<ColorWriteChannels>(value.ColorWriteChannels);
            output.WriteObject<ColorWriteChannels>(value.ColorWriteChannels1);
            output.WriteObject<ColorWriteChannels>(value.ColorWriteChannels2);
            output.WriteObject<ColorWriteChannels>(value.ColorWriteChannels3);
            output.WriteObject<int>(value.MultiSampleMask);
        }

        public override string GetRuntimeType(TargetPlatform targetPlatform)
        {
            return typeof(BlendState).AssemblyQualifiedName;
        }

        public override string GetRuntimeReader(TargetPlatform targetPlatform)
        {
            return "Myre.Graphics.ContentPipeline.BlendStateReader, Myre.Graphics";
        }
    }
}
