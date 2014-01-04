using System.Collections.Generic;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;

namespace Myre.Graphics.Pipeline.Animations
{
    [ContentSerializerRuntimeType("Myre.Graphics.Animation.Clip, Myre.Graphics")]
    public class ClipContent
    {
        public string Name { get; private set; }
        public List<KeyframeContent> Keyframes { get; private set; }

        public ClipContent(string name)
        {
            Name = name;
            Keyframes = new List<KeyframeContent>();
        }
    }

    [ContentTypeWriter]
    public class ClipContentWriter : ContentTypeWriter<ClipContent>
    {
        protected override void Write(ContentWriter output, ClipContent value)
        {
            output.Write(value.Name);

            output.Write(value.Keyframes.Count);
            for (int i = 0; i < value.Keyframes.Count; i++)
                output.WriteObject(value.Keyframes[i]);
        }

        public override string GetRuntimeReader(TargetPlatform targetPlatform)
        {
            return "Myre.Graphics.Animation.Clips.ClipReader, Myre.Graphics";
        }

        public override string GetRuntimeType(TargetPlatform targetPlatform)
        {
            return "Myre.Graphics.Animation.Clips.Clip, Myre.Graphics";
        }
    }
}
