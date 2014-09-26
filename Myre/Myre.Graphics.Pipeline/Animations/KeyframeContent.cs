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
        public ushort Bone { get; set; }
        public TimeSpan Time { get; set; }

        public Vector3 Translation { get; set; }
        public Vector3 Scale { get; set; }
        public Quaternion Rotation { get; set; }

        public KeyframeContent(ushort boneIndex, TimeSpan timeSpan, Vector3 position, Vector3 scale, Quaternion orientaton)
        {
            Bone = boneIndex;
            Time = timeSpan;

            Translation = position;
            Scale = scale;
            Rotation = orientaton;
        }
    }

    [ContentTypeWriter]
    public class KeyframeContentWriter : ContentTypeWriter<KeyframeContent>
    {
        protected override void Write(ContentWriter output, KeyframeContent value)
        {
            output.Write(value.Bone);
            output.Write(value.Time.Ticks);

            output.Write(value.Translation);
            output.Write(value.Scale);
            output.Write(value.Rotation);
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
