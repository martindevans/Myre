using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Myre.Collections;
using Myre.Graphics;
using Myre.Graphics.Materials;
using Ninject;

namespace GraphicsTests.Tests
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
        private Material _material;
        private Quad _quad;
        private NamedBoxCollection _metadata;
        private readonly ContentManager _content;
        private readonly GraphicsDevice _device;

        public MaterialParametersTest(
            IKernel kernel,
            ContentManager content,
            GraphicsDevice device)
            : base("Material Parameters", kernel)
        {
            _content = content;
            _device = device;
        }

        protected override void BeginTransitionOn()
        {
            _material = new Material(_content.Load<Effect>("Basic"), null);
            _quad = new Quad(_device);
            _metadata = new NamedBoxCollection();

            _metadata.Set("colour", Color.White.ToVector4());

            base.OnShown();
        }

        public override void Update(GameTime gameTime)
        {
            var time = gameTime.TotalGameTime.TotalSeconds;
            _metadata.Set("colour", new Vector4((float)Math.Sin(time), (float)Math.Sin(time * 2), (float)Math.Sin(time * 3), 1f));
            
            base.Update(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
            _quad.Draw(_material, _metadata);

            base.Draw(gameTime);
        }
    }
}
