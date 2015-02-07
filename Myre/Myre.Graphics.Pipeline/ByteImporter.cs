using System.Collections.Generic;
using Microsoft.Xna.Framework.Content.Pipeline;
using System.IO;

namespace Myre.Graphics.Pipeline
{
    public abstract class ByteImporter
        : ContentImporter<KeyValuePair<string, byte[]>>
    {
        public override KeyValuePair<string, byte[]> Import(string filename, ContentImporterContext context)
        {
            return new KeyValuePair<string, byte[]>(filename, File.ReadAllBytes(filename));
        }
    }
}
