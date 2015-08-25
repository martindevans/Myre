using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;

using Color = Microsoft.Xna.Framework.Color;

namespace Myre.Graphics.Pipeline.Particles.Initialisers.Colour
{
    public class RandomEndColour
        :IInitialiser
    {
        public Color Min { get; set; }
        public Color Max { get; set; }
    }

    [ContentTypeWriter]
    public class RandomEndColourWriter
        : ContentTypeWriter<RandomEndColour>
    {
        protected override void Write(ContentWriter output, RandomEndColour value)
        {
            output.Write(value.Min);
            output.Write(value.Max);
        }

        public override string GetRuntimeType(TargetPlatform targetPlatform)
        {
            return "Myre.Graphics.Translucency.Particles.Initialisers.Colour.RandomEndColour, Myre.Graphics";
        }

        public override string GetRuntimeReader(TargetPlatform targetPlatform)
        {
            return "Myre.Graphics.Translucency.Particles.Initialisers.Colour.RandomEndColourReader, Myre.Graphics";
        }
    }
}
