using System;
using Microsoft.Xna.Framework.Content;

namespace Myre.Graphics.Animation.Clips
{
    [Serializable]
    public class Clip
        :IClip
    {
        public string Name { get; internal set; }
        public Keyframe[][] Channels { get; internal set; }

        public TimeSpan Duration { get; internal set; }

        public void Start()
        {
        }

        internal Clip()
        {
        }
    }

    public class ClipReader : ContentTypeReader<Clip>
    {
        protected override Clip Read(ContentReader input, Clip existingInstance)
        {
            existingInstance = existingInstance ?? new Clip();

            existingInstance.Name = input.ReadString();

            existingInstance.Duration = new TimeSpan(input.ReadInt64());

            int count = input.ReadInt32();
            existingInstance.Channels = new Keyframe[count][];
            for (int i = 0; i < count; i++)
            {
                existingInstance.Channels[i] = new Keyframe[input.ReadInt32()];
                for (int j = 0; j < existingInstance.Channels[i].Length; j++)
                    existingInstance.Channels[i][j] = input.ReadObject<Keyframe>();
            }

            return existingInstance;
        }
    }
}
