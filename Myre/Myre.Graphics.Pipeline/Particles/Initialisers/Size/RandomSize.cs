
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;

namespace Myre.Graphics.Pipeline.Particles.Initialisers.Size
{
    public class RandomSize
        :IInitialiser
    {
        public float Min { get; set; }
        public float Max { get; set; }
    }

    [ContentTypeWriter]
    public class RandomSizeWriter
        : ContentTypeWriter<RandomSize>
    {
        protected override void Write(ContentWriter output, RandomSize value)
        {
            output.Write(value.Min);
            output.Write(value.Max);
        }

        public override string GetRuntimeType(TargetPlatform targetPlatform)
        {
            return "Myre.Graphics.Translucency.Particles.Initialisers.Size.RandomSize, Myre.Graphics";
        }

        public override string GetRuntimeReader(TargetPlatform targetPlatform)
        {
            return "Myre.Graphics.Translucency.Particles.Initialisers.Size.RandomSizeReader, Myre.Graphics";
        }
    }
}
