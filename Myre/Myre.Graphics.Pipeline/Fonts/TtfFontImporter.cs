
using Microsoft.Xna.Framework.Content.Pipeline;

namespace Myre.Graphics.Pipeline.Fonts
{
    [ContentImporter(".ttf", DefaultProcessor = "VertexFontProcessor", CacheImportedData = true, DisplayName = "TTF Importer")]
    public class TtfFontImporter
        : ByteImporter
    {
    }
}
