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

namespace GraphicsTests
{
    class MaterialContentTest
        : TestScreen
    {
        private Material material;
        private Quad quad;
        private BoxedValueStore<string> metadata;
        private ContentManager content;
        private GraphicsDevice device;

        public MaterialContentTest(
            IKernel kernel,
            ContentManager content,
            GraphicsDevice device)
            : base("Material Content Loading", kernel)
        {
            this.content = content;
            this.device = device;
        }

        protected override void BeginTransitionOn()
        {
            material = content.Load<Material>("Red");
            quad = new Quad(device);
            metadata = new BoxedValueStore<string>();

            base.OnShown();
        }

        public override void Draw(Microsoft.Xna.Framework.GameTime gameTime)
        {
            quad.Draw(material, metadata);

            base.Draw(gameTime);
        }
    }
}
