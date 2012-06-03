using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ninject;
using Myre.Graphics.Materials;
using Microsoft.Xna.Framework.Content;
using Myre.Graphics;
using Myre.Collections;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System.Diagnostics;
using System.Xml;

namespace GraphicsTests
{
    public class MyreMaterialData
    {
        public string EffectName;
        public Dictionary<string, string> Textures = new Dictionary<string, string>();
        public Dictionary<string, object> OpaqueData = new Dictionary<string, object>();
        public string Technique;
    }

    class MaterialParametersTest
        : TestScreen
    {
        private Material material;
        private Quad quad;
        private BoxedValueStore<string> metadata;
        private ContentManager content;
        private GraphicsDevice device;

        public MaterialParametersTest(
            IKernel kernel,
            ContentManager content,
            GraphicsDevice device)
            : base("Material Parameters", kernel)
        {
            this.content = content;
            this.device = device;
        }

        protected override void BeginTransitionOn()
        {
            material = new Material(content.Load<Effect>("Basic"), null);
            quad = new Quad(device);
            metadata = new BoxedValueStore<string>();

            metadata.Set("colour", Color.White.ToVector4());

            base.OnShown();
        }

        public override void Update(Microsoft.Xna.Framework.GameTime gameTime)
        {
            var time = gameTime.TotalGameTime.TotalSeconds;
            metadata.Set("colour", new Vector4((float)Math.Sin(time), (float)Math.Sin(time * 2), (float)Math.Sin(time * 3), 1f));
            
            base.Update(gameTime);
        }

        public override void Draw(Microsoft.Xna.Framework.GameTime gameTime)
        {
            quad.Draw(material, metadata);

            base.Draw(gameTime);
        }
    }
}
