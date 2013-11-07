using System;
using System.Linq;
using Microsoft.Xna.Framework.Content;

namespace Myre.Graphics.Animation.Clips
{
    public class Clip
        :IClip
    {
        public string Name { get; internal set; }
        public Keyframe[] Keyframes { get; internal set; }

        public TimeSpan Duration
        {
            get { return Keyframes.Last().Time; }
        }

        public void Start()
        {
        }

        internal Clip()
        {
        }

        public Clip(Keyframe[] keyframes)
        {
            Keyframes = keyframes;
        }
    }

    public class ClipReader : ContentTypeReader<Clip>
    {
        protected override Clip Read(ContentReader input, Clip existingInstance)
        {
            existingInstance = existingInstance ?? new Clip();

            existingInstance.Name = input.ReadString();

            int count = input.ReadInt32();
            existingInstance.Keyframes = new Keyframe[count];
            for (int i = 0; i < count; i++)
                existingInstance.Keyframes[i] = input.ReadObject<Keyframe>();

            return existingInstance;
        }
    }
}
