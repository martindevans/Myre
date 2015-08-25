//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using Myre.Graphics.Materials;
//using Microsoft.Xna.Framework.Graphics;
//using Microsoft.Xna.Framework.Content;

//namespace Myre.Graphics.Lighting
//{
//    public class GammaCorrectionPhase
//       : RenderPhase
//    {
//        protected override void SpecifyResources(IList<string> inputs, IList<RenderPhase.Output> outputs)
//        {
//            inputs.Add("tonemapped");
//            outputs.Add(new Output() { Name = "gammaspace" });
//        }

//        Quad quad;
//        Material gammaCorrect;

//        public GammaCorrectionPhase(
//            GraphicsDevice device,
//            ContentManager content)
//        {
//            quad = new Quad(device);
//            var effect = content.Load<Effect>("Gamma");
//            gammaCorrect = new Material(effect, null);
//        }

//        public override void Draw(Renderer renderer)
//        {
//            var metadata = renderer.Metadata;
//            var device = renderer.Device;

//            var resolution = metadata.GetValue<Vector2>("resolution");

//            var target = RenderTargetManager.GetTarget(device,
//                                                        (int)resolution.X,
//                                                        (int)resolution.Y,
//                                                        SurfaceFormat.HdrBlendable);

//            device.SetRenderTarget(target);
//            device.BlendState = BlendState.Opaque;
//            quad.Draw(gammaCorrect, metadata);

//            renderer.SetResource("gammaspace", target);
//        }
//    }
//}
