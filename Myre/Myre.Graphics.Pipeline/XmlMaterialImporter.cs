using System.Xml;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Intermediate;

namespace Myre.Graphics.Pipeline
{
    /// <summary>
    /// This class will be instantiated by the XNA Framework Content Pipeline
    /// to import a file from disk into the specified type, TImport.
    /// 
    /// This should be part of a Content Pipeline Extension Library project.
    /// 
    /// TODO: change the ContentImporter attribute to specify the correct file
    /// extension, display name, and default processor for this importer.
    /// </summary>
    [ContentImporter(".mat", DisplayName = "Xml Myre Material Importer", DefaultProcessor = "MyreMaterialProcessor")]
    public class XmlMaterialImporter : ContentImporter<MyreMaterialData>
    {
        public override MyreMaterialData Import(string filename, ContentImporterContext context)
        {
            var reader = XmlReader.Create(filename);
            return IntermediateSerializer.Deserialize<MyreMaterialData>(reader, null);
        }
    }
}
