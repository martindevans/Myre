using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using System.Collections.Generic;
using System.Linq;

namespace Myre.Graphics.Pipeline.Animations
{
    [ContentProcessor(DisplayName = "Myre Embedded Animation Processor")]
    public class EmbeddedAnimationProcessor
        : BaseAnimationProcessor<MyreEmbeddedAnimationDefinition>
    {
        public override ClipContent Process(MyreEmbeddedAnimationDefinition input, ContentProcessorContext context)
        {
            //Build the content source
            NodeContent node = context.BuildAndLoadAsset<NodeContent, NodeContent>(new ExternalReference<NodeContent>(input.AnimationSourceFile), null);

            //Find the named animation from the content source
            var animation = FindAnimations(node).Where(a => a.Key == input.SourceTakeName).Select(a => a.Value).FirstOrDefault();
            if (animation == null)
                throw new KeyNotFoundException(string.Format(@"Animation '{0}' not found, only options are {1}", input.SourceTakeName, FindAnimations(node).Select(a => a.Key).Aggregate((a, b) => a + "," + b)));

            return Process(node, animation, input.RootBone, input.StartTime, input.EndTime, input.FixLooping, input.LinearKeyframeReduction, null);
        }
    }
}
