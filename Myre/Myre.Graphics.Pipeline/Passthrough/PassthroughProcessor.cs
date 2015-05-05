using Microsoft.Xna.Framework.Content.Pipeline;
using System.Collections.Generic;

namespace Myre.Graphics.Pipeline.Passthrough
{
    [ContentProcessor(DisplayName = "Passthrough Processor")]
    public class PassthroughProcessor
        : ContentProcessor<KeyValuePair<string, byte[]>, byte[]>
    {
        public override byte[] Process(KeyValuePair<string, byte[]> input, ContentProcessorContext context)
        {
            return input.Value;
        }
    }
}
