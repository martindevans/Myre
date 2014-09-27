using System;
using Microsoft.Xna.Framework.Content;

namespace Myre.Graphics.Animation.Clips
{
    [Serializable]
    public class Clip
        :IClip
    {
        public string Name { get; private set; }

        public Keyframe[][] Channels { get; private set; }

        public int ChannelCount { get { return Channels.Length; } }

        public TimeSpan Duration { get; private set; }

        public ushort RootBoneIndex { get; private set; }

        public void Start()
        {
        }

        internal Clip(string name, TimeSpan duration, Keyframe[][] channels, ushort rootBoneIndex)
        {
            Name = name;
            Channels = channels;
            Duration = duration;
            RootBoneIndex = rootBoneIndex;
        }

        public int FindChannelFrameIndex(int channel, int startIndex, TimeSpan elapsedTime)
        {
            var frames = Channels[channel];
            var index = startIndex;

            //Iterate up frames until we find the frame which is greater than the current time index for this channel
            while (frames[index].Time <= elapsedTime)
                index++;

            return index;
        }
    }

    public class ClipReader : ContentTypeReader<Clip>
    {
        protected override Clip Read(ContentReader input, Clip existingInstance)
        {
            return new Clip(
                input.AssetName,
                new TimeSpan(input.ReadInt64()),
                ReadChannels(input),
                input.ReadUInt16()
            );
        }

        private Keyframe[][] ReadChannels(ContentReader input)
        {
            int count = input.ReadInt32();
            var channels = new Keyframe[count][];

            for (int i = 0; i < count; i++)
            {
                channels[i] = new Keyframe[input.ReadInt32()];
                for (int j = 0; j < channels[i].Length; j++)
                    channels[i][j] = input.ReadObject<Keyframe>();
            }

            return channels;
        }
    }
}
