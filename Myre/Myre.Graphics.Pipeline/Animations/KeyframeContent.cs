using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;

namespace Myre.Graphics.Pipeline.Animations
{
    [ContentSerializerRuntimeType("Myre.Graphics.Animation.Keyframe, Myre.Graphics")]
    public class KeyframeContent
    {
        public int Bone { get; set; }
        public TimeSpan Time { get; set; }
        public Matrix Transform { get; set; }

        public KeyframeContent(int boneIndex, TimeSpan timeSpan, Matrix matrix)
        {
            Bone = boneIndex;
            Time = timeSpan;
            Transform = matrix;
        }
    }

    [ContentTypeWriter]
    public class KeyframeContentWriter : ContentTypeWriter<KeyframeContent>
    {
        protected override void Write(ContentWriter output, KeyframeContent value)
        {
            output.Write(value.Bone);
            output.Write(value.Time.Ticks);
            output.Write(value.Transform);
        }

        public override string GetRuntimeReader(TargetPlatform targetPlatform)
        {
            return "Myre.Graphics.Animation.KeyframeReader, Myre.Graphics";
        }

        public override string GetRuntimeType(TargetPlatform targetPlatform)
        {
            return "Myre.Graphics.Animation.Keyframe, Myre.Graphics";
        }
    }
}
