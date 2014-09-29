using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework.Content;

namespace Myre.Graphics.Animation.Clips
{
    [Serializable]
    public class Clip
        :IClip
    {
        public string Name { get; private set; }

        private readonly Channel[] _channels;

        public int ChannelCount { get { return _channels.Length; } }

        public TimeSpan Duration { get; private set; }

        public ushort RootBoneIndex { get; private set; }

        public void Start()
        {
        }

        internal Clip(string name, TimeSpan duration, IEnumerable<Keyframe[]> channels, ushort rootBoneIndex)
        {
            Name = name;
            _channels = channels.Select((a, i) => new Channel(i, a)).ToArray();
            Duration = duration;
            RootBoneIndex = rootBoneIndex;
        }

        public IChannel GetChannel(int index)
        {
            return _channels[index];
        }
    }

    public class Channel
        : IChannel
    {
        private readonly Keyframe[] _frames;

        public int BoneIndex { get; private set; }

        public Channel(int boneIndex, Keyframe[] frames)
        {
            _frames = frames;
            BoneIndex = boneIndex;
        }

        public Keyframe BoneTransform(int index)
        {
            return _frames[index];
        }

        public int SeekToTimestamp(TimeSpan elapsedTime, int startIndex = 0)
        {
            var index = startIndex;

            //Iterate up frames until we find the frame which is greater than the current time index for this channel
            while (_frames[index].Time <= elapsedTime && index < _frames.Length)
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

        private static IEnumerable<Keyframe[]> ReadChannels(ContentReader input)
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
