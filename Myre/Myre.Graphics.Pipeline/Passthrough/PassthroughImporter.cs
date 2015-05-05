using Microsoft.Xna.Framework.Content.Pipeline;

namespace Myre.Graphics.Pipeline.Passthrough
{
    [ContentImporter(".*", DefaultProcessor = "PassthroughProcessor", CacheImportedData = true, DisplayName = "Passthrough Importer")]
    public class PassthroughImporter
        : ByteImporter
    {
    }
}
