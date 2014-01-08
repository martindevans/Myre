using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;
using System.Linq;

namespace Myre.Graphics.Pipeline.Animations
{
    [ContentSerializerRuntimeType("Myre.Graphics.Animation.Clip, Myre.Graphics")]
    public class ClipContent
    {
        public string Name { get; private set; }
        public int RootBoneIndex { get; private set; }
        public List<List<KeyframeContent>> Channels { get; private set; }

        public ClipContent(string name, int boneCount, int rootBoneIndex)
        {
            Name = name;
            RootBoneIndex = rootBoneIndex;

            Channels = new List<List<KeyframeContent>>();
            for (int i = 0; i < boneCount; i++)
                Channels.Add(new List<KeyframeContent>());
        }

        public void SortKeyframes()
        {
            Parallel.ForEach(Channels, k => k.Sort((a, b) => a.Time.CompareTo(b.Time)));
        }

        public void SubtractKeyframeTime()
        {
            var min = Channels.Select(a => a.Min(k => k.Time)).Min();
            Parallel.ForEach(Channels, c => c.ForEach(k => k.Time -= min));
        }

        public void InsertStartFrames()
        {
            Parallel.ForEach(Channels, c =>
            {
                var first = c[0];
                if (first.Time.Ticks != 0)
                    c.Insert(0, new KeyframeContent(first.Bone, new TimeSpan(0), first.Translation, first.Scale, first.Rotation));
            });
        }
    }

    [ContentTypeWriter]
    public class ClipContentWriter : ContentTypeWriter<ClipContent>
    {
        protected override void Write(ContentWriter output, ClipContent value)
        {
            //Name of this animation
            output.Write(value.Name);

            //Time index of the last keyframe of this animation
            output.Write(value.Channels.Select(c => c.Max(k => k.Time)).Max().Ticks);

            //Keyframes
            output.Write(value.Channels.Count);
            for (int i = 0; i < value.Channels.Count; i++)
            {
                output.Write(value.Channels[i].Count);
                for (int j = 0; j < value.Channels[i].Count; j++)
                    output.WriteObject(value.Channels[i][j]);
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
