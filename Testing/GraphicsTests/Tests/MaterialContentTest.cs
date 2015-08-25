using Ninject;
using Myre.Graphics.Materials;
using Microsoft.Xna.Framework.Content;
using Myre.Graphics;
using Myre.Collections;
using Microsoft.Xna.Framework.Graphics;

using GameTime = Microsoft.Xna.Framework.GameTime;

namespace GraphicsTests.Tests
{
    class MaterialContentTest
        : TestScreen
    {
        private Material _material;
        private Quad _quad;
        private NamedBoxCollection _metadata;
        private readonly ContentManager _content;
        private readonly GraphicsDevice _device;

        public MaterialContentTest(
            IKernel kernel,
            ContentManager content,
            GraphicsDevice device)
            : base("Material Content Loading", kernel)
        {
            _content = content;
            _device = device;
        }

        protected override void BeginTransitionOn()
        {
            _material = _content.Load<Material>("Red");
            _quad = new Quad(_device);
            _metadata = new NamedBoxCollection();

            base.OnShown();
        }

        public override void Draw(GameTime gameTime)
        {
            _quad.Draw(_material, _metadata);

            base.Draw(gameTime);
        }
    }
}
