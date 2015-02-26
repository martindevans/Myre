using Microsoft.Xna.Framework.Content.Pipeline;

namespace Myre.Graphics.Pipeline
{
    public class XmlImporter<T>
        : XmlImporter
        where T : ContentItem
    {
        public override object Import(string filename, ContentImporterContext context)
        {
            var b = (T)base.Import(filename, context);

            b.Identity = new ContentIdentity(filename);

            return b;
        }
    }
}
