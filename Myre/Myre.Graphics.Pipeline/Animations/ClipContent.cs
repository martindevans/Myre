using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;

namespace Myre.Graphics.Pipeline.Animations
{
    [ContentSerializerRuntimeType("Myre.Graphics.Animation.Clip, Myre.Graphics")]
    public class ClipContent
    {
        public ushort RootBoneIndex { get; private set; }
        public Channel[] Channels { get; private set; }

        public ClipContent(int boneCount, ushort rootBoneIndex)
        {
            RootBoneIndex = rootBoneIndex;

            Channels = new Channel[boneCount];
        }

        public void SortKeyframes()
        {
            Parallel.ForEach(Channels, k => k.Keyframes.Sort((a, b) => a.Time.CompareTo(b.Time)));
        }

        public void SubtractKeyframeTime()
        {
            var min = Channels.Select(a => a.Keyframes.Min(k => k.Time)).Min();
            Parallel.ForEach(Channels, c =>
            {
                foreach (var t in c.Keyframes)
                    t.Time -= min;
            });
        }

        public void InsertStartAndEndFrames()
        {
            var endTime = Channels.Select(c => c.Keyframes.Max(k => k.Time)).Max().Ticks;

            Parallel.ForEach(Channels, (c, i) =>
            {
                var first = c.Keyframes.First();
                var last = c.Keyframes.Last();
                var insertFirst = first.Time.Ticks != 0;
                var insertLast = last.Time.Ticks != endTime;

                var expansionCount = (insertFirst ? 1 : 0) + (insertLast ? 1 : 0);
                if (expansionCount != 0)
                {
                    //Insert a keyframe at the start
                    if (insertFirst)
                        c.Keyframes.Insert(0, new KeyframeContent(first.Bone, new TimeSpan(0), first.Translation, first.Scale, first.Rotation));

                    //Insert a keyframe at the end
                    if (insertLast)
                        c.Keyframes.Add(new KeyframeContent(last.Bone, new TimeSpan(endTime), last.Translation, last.Scale, last.Rotation));
                }
            });
        }
    }

    public class Channel
    {
        public readonly List<KeyframeContent> Keyframes;
        public readonly float Weight;

        public Channel(List<KeyframeContent> keyframes, float weight)
        {
            Keyframes = keyframes;
            Weight = weight;
        }
    }

    [ContentTypeWriter]
    public class ClipContentWriter : ContentTypeWriter<ClipContent>
    {
        protected override void Write(ContentWriter output, ClipContent value)
        {
            //Time index of the last keyframe of this animation
            output.Write(value.Channels.Select(c => c.Keyframes).Select(c => c.Max(k => k.Time)).Max().Ticks);

            //Keyframes
            output.Write(value.Channels.Length);
            foreach (var channel in value.Channels)
            {
                //todo: channel weight!
                output.Write(channel.Weight);

                output.Write(channel.Keyframes.Count);
                for (var j = 0; j < channel.Keyframes.Count; j++)
                    output.WriteObject(channel.Keyframes[j]);
            }

            //The root bone index
            output.Write(value.RootBoneIndex);
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
