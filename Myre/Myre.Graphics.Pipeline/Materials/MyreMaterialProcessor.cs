using Microsoft.Xna.Framework.Content.Pipeline;

namespace Myre.Graphics.Pipeline.Materials
{
    /// <summary>
    /// This class will be instantiated by the XNA Framework Content Pipeline
    /// to apply custom processing to content data, converting an object of
    /// type TInput to TOutput. The input and output types may be the same if
    /// the processor wishes to alter data without changing its type.
    /// </summary>
    [ContentProcessor(DisplayName = "Myre Material Processor")]
    public class MyreMaterialProcessor : ContentProcessor<MyreMaterialDefinition, MyreMaterialContent>
    {
        public override MyreMaterialContent Process(MyreMaterialDefinition input, ContentProcessorContext context)
        {
            MyreMaterialContent output = new MyreMaterialContent
            {
                Technique = input.Technique,
                EffectName = input.EffectName,
            };

            foreach (var texture in input.Textures)
                output.Textures.Add(texture.Key, texture.Value);

            foreach (var item in input.OpaqueData)
                output.OpaqueData.Add(item.Key, item.Value);

            return output;
        }
    }
}