using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Myre.Graphics.Materials
{
    /// <summary>
    /// This class will be instantiated by the XNA Framework Content
    /// Pipeline to read the specified data type from binary .xnb format.
    /// 
    /// Unlike the other Content Pipeline support classes, this should
    /// be a part of your main game project, and not the Content Pipeline
    /// Extension Library project.
    /// </summary>
    public class MaterialReader : ContentTypeReader<Material>
    {
        protected override Material Read(ContentReader input, Material existingInstance)
        {
            string technique = input.ReadString();
            if (technique == "")
                technique = null;

            Effect effect = input.ReadObject<Effect>();

            return new Material(effect, technique);
        }
    }
}
