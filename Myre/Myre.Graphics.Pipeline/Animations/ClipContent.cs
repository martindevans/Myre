using System;
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
        public string Name { get; private set; }
        public int RootBoneIndex { get; private set; }
        public KeyframeContent[][] Channels { get; private set; }

        public ClipContent(string name, int boneCount, int rootBoneIndex)
        {
            Name = name;
            RootBoneIndex = rootBoneIndex;

            Channels = new KeyframeContent[boneCount][];
        }

        public void SortKeyframes()
        {
            Parallel.ForEach(Channels, k => Array.Sort(k, (a, b) => a.Time.CompareTo(b.Time)));
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

        public void InsertStartFrames()
        {
            Parallel.ForEach(Channels, c =>
            {
                var first = c[0];
                if (first.Time.Ticks != 0)
                {
                    //Create a new array 1 longer
                    var n = new KeyframeContent[c.Length + 1];

                    //Insert a keyframe at the start
                    n[0] = new KeyframeContent(first.Bone, new TimeSpan(0), first.Translation, first.Scale, first.Rotation);

                    //Copy over the rest
                    Array.Copy(c, 0, n, 1, c.Length);
                }
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
            output.Write(value.Channels.Length);
            for (int i = 0; i < value.Channels.Length; i++)
            {
                output.Write(value.Channels[i].Length);
                for (int j = 0; j < value.Channels[i].Length; j++)
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
