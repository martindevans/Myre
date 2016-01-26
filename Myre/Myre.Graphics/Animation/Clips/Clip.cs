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

        internal Clip(string name, TimeSpan duration, IEnumerable<KeyValuePair<Keyframe[], float>> channels, ushort rootBoneIndex)
        {
            Name = name;
            _channels = channels.Select((a, i) => new Channel((ushort)i, a.Key, a.Value)).ToArray();
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

        private readonly float _weight;

        public ushort BoneIndex { get; private set; }

        public Channel(ushort boneIndex, Keyframe[] frames, float weight)
        {
            _frames = frames;
            _weight = weight;

            BoneIndex = boneIndex;
        }

        public Keyframe BoneTransform(int index)
        {
            return _frames[index];
        }

        public float TransformWeight(int index)
        {
            //Weight could change with time, but that isn't supported in this implementation and it is fixed per channel
            //That's why we're using a method to return a readonly value
            return _weight;
        }

        public int SeekToTimestamp(TimeSpan elapsedTime, int startIndex = 0)
        {
            var index = startIndex;

            //Iterate up frames until we find the frame which is greater than the current time index for this channel
            while (_frames[index].Time <= elapsedTime && index < _frames.Length - 1)
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

        private static IEnumerable<KeyValuePair<Keyframe[], float>> ReadChannels(ContentReader input)
        {
            var count = input.ReadInt32();
            var channels = new KeyValuePair<Keyframe[], float>[count];

            for (var i = 0; i < count; i++)
            {
                var weight = input.ReadSingle();
                var keyframes = new Keyframe[input.ReadInt32()];
                var channel = new KeyValuePair<Keyframe[], float>(keyframes, weight);

                for (int j = 0; j < channel.Key.Length; j++)
                    channel.Key[j] = input.ReadObject<Keyframe>();

                channels[i] = channel;
            }

            return channels;
        }
    }
}
