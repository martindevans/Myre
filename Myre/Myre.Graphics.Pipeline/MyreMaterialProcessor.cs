using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;

namespace Myre.Graphics.Pipeline
{
    /// <summary>
    /// This class will be instantiated by the XNA Framework Content Pipeline
    /// to apply custom processing to content data, converting an object of
    /// type TInput to TOutput. The input and output types may be the same if
    /// the processor wishes to alter data without changing its type.
    ///
    /// This should be part of a Content Pipeline Extension Library project.
    ///
    /// TODO: change the ContentProcessor attribute to specify the correct
    /// display name for this processor.
    /// </summary>
    [ContentProcessor(DisplayName = "Myre Material Processor")]
    public class MyreMaterialProcessor : ContentProcessor<MyreMaterialData, MyreMaterialContent>
    {
        public override MyreMaterialContent Process(MyreMaterialData input, ContentProcessorContext context)
        {
            MyreMaterialContent output = new MyreMaterialContent {Technique = input.Technique};

            EffectMaterialContent material = new EffectMaterialContent {Effect = new ExternalReference<EffectContent>(input.EffectName)};

            foreach (var texture in input.Textures)
                material.Textures.Add(texture.Key, new ExternalReference<TextureContent>(texture.Value));

            foreach (var item in input.OpaqueData)
                material.OpaqueData.Add(item.Key, item.Value);

            var processorParameters = new OpaqueDataDictionary();
            processorParameters.Add("PremultiplyTextureAlpha", false);
            output.Material = context.Convert<MaterialContent, MaterialContent>(material, "MaterialProcessor", processorParameters);

            return output;
        }
    }
}