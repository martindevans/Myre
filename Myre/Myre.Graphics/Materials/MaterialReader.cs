using System.Collections.Generic;
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

            string effectName = input.ReadString();
            Effect effect = input.ContentManager.Load<Effect>(effectName).Clone();

            var material = new Material(effect, technique);

            int count = input.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                var name = input.ReadString();
                var path = input.ReadString();

                material.Parameters[name].SetValue(input.ContentManager.Load<Texture2D>(path));
            }

            Dictionary<string, object> opaqueData = new Dictionary<string, object>();
            int opaqueCount = input.ReadInt32();
            for (int i = 0; i < opaqueCount; i++)
            {
                var key = input.ReadString();
                var data = input.ReadObject<object>();

                opaqueData[key] = data;
            }

            return material;
        }
    }
}
