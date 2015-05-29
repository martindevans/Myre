using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using System.IO;

namespace Myre.Graphics.Pipeline.Animations.BVH
{
    [ContentImporter(".bvh", DefaultProcessor = "AnimationProcessor", CacheImportedData = true, DisplayName = "BVH Animation Importer")]
    public class BvhImporter
        : ContentImporter<NodeContent>
    {
        public override NodeContent Import(string filename, ContentImporterContext context)
        {
            return new BvhParser(File.ReadAllLines(filename))
                .Parse(Path.GetFileNameWithoutExtension(filename));
        }
    }
}
