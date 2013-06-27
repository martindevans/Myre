using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

namespace Myre.Graphics.Animation
{
    public class Keyframe
    {
        public int Bone { get; set; }
        public TimeSpan Time { get; set; }
        public Matrix Transform { get; set; }
    }

    public class KeyframeReader : ContentTypeReader<Keyframe>
    {
        protected override Keyframe Read(ContentReader input, Keyframe existingInstance)
        {
            existingInstance = existingInstance ?? new Keyframe();

            existingInstance.Bone = input.ReadInt32();
            existingInstance.Time = new TimeSpan(input.ReadInt64());
            existingInstance.Transform = input.ReadMatrix();

            return existingInstance;
        }
    }
}
