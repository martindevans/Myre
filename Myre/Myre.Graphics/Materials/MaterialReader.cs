using System;
using Microsoft.Xna.Framework;
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
            //Effect effect = input.ReadObject<Effect>();
            Effect effect = input.ContentManager.Load<Effect>(effectName).Clone();

            var material = new Material(effect, technique);

            int count = input.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                var name = input.ReadString();
                var path = input.ReadString();

                material.Parameters[name].SetValue(input.ContentManager.Load<Texture2D>(path));
            }

            int opaqueCount = input.ReadInt32();
            for (int i = 0; i < opaqueCount; i++)
            {
                var key = input.ReadString();
                var data = input.ReadObject<object>();

                var p = material.Parameters[key];
                if (p != null)
                {
                    var t = data.GetType();

                    if (t == typeof(bool))
                        p.SetValue((bool)data);
                    else if (t == typeof(bool[]))
                        p.SetValue((bool[])data);

                    else if (t == typeof(float))
                        p.SetValue((float)data);
                    else if (t == typeof(float[]))
                        p.SetValue((float[])data);

                    else if (t == typeof(int))
                        p.SetValue((int)data);
                    else if (t == typeof(int[]))
                        p.SetValue((int[])data);
                    
                    else if (t == typeof(Matrix))
                        p.SetValue((Matrix) data);
                    else if (t == typeof(Matrix[]))
                        p.SetValue((Matrix[]) data);

                    else if (t == typeof(Quaternion))
                        p.SetValue((Quaternion) data);
                    else if (t == typeof(Quaternion[]))
                        p.SetValue((Quaternion[]) data);

                    else if (t == typeof(string))
                        p.SetValue((string)data);

                    //This shouldn't ever happen, if it does it means a texture was encoded directly into the material file (ugh)
                    //However, just in case, we'll leave this here :P
                    else if (t == typeof(Texture))
                        p.SetValue((Texture)data);

                    else if (t == typeof(Vector2))
                        p.SetValue((Vector2)data);
                    else if (t == typeof(Vector2[]))
                        p.SetValue((Vector2[])data);

                    else if (t == typeof(Vector3))
                        p.SetValue((Vector3)data);
                    else if (t == typeof(Vector3[]))
                        p.SetValue((Vector3[])data);

                    else if (t == typeof(Vector4))
                        p.SetValue((Vector4)data);
                    else if (t == typeof(Vector4[]))
                        p.SetValue((Vector4[])data);

                    else
                        throw new InvalidOperationException(string.Format("Unknown effect parameter type {0}", t.Name));
                }
                else
                    throw new InvalidOperationException(string.Format("Effect parameter {0} not found", key));
            }

            return material;
        }
    }
}
