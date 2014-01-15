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
        public int RootBoneIndex { get; private set; }
        public List<KeyframeContent>[] Channels { get; private set; }

        public ClipContent(int boneCount, int rootBoneIndex)
        {
            RootBoneIndex = rootBoneIndex;

            Channels = new List<KeyframeContent>[boneCount];
        }

        public void SortKeyframes()
        {
            Parallel.ForEach(Channels, k => k.Sort((a, b) => a.Time.CompareTo(b.Time)));
        }

        public void SubtractKeyframeTime()
        {
            var min = Channels.Select(a => a.Min(k => k.Time)).Min();
            Parallel.ForEach(Channels, c =>
            {
                foreach (KeyframeContent t in c)
                    t.Time -= min;
            });
        }

        public void InsertStartAndEndFrames()
        {
            var endTime = Channels.Select(c => c.Max(k => k.Time)).Max().Ticks;

            Parallel.ForEach(Channels, (c, i) =>
            {
                var first = c.First();
                var last = c.Last();
                bool insertFirst = first.Time.Ticks != 0;
                bool insertLast = last.Time.Ticks != endTime;

                int expansionCount = (insertFirst ? 1 : 0) + (insertLast ? 1 : 0);
                
                if (expansionCount != 0)
                {
                    //Insert a keyframe at the start
                    if (insertFirst)
                        c.Insert(0, new KeyframeContent(first.Bone, new TimeSpan(0), first.Translation, first.Scale, first.Rotation));

                    //Insert a keyframe at the end
                    if (insertLast)
                        c.Add(new KeyframeContent(last.Bone, new TimeSpan(endTime), last.Translation, last.Scale, last.Rotation));
                }
            });
        }
    }

    [ContentTypeWriter]
    public class ClipContentWriter : ContentTypeWriter<ClipContent>
    {
        protected override void Write(ContentWriter output, ClipContent value)
        {
            //Time index of the last keyframe of this animation
            output.Write(value.Channels.Select(c => c.Max(k => k.Time)).Max().Ticks);

            //Keyframes
            output.Write(value.Channels.Length);
            foreach (var channel in value.Channels)
            {
                output.Write(channel.Count);
                for (int j = 0; j < channel.Count; j++)
                    output.WriteObject(channel[j]);
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
