using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;

namespace Myre.Graphics.Pipeline
{
    #region Clip
    [ContentSerializerRuntimeType("Myre.Graphics.Animation.Clip, Myre.Graphics")]
    public class MyreClipContent
    {
        public string Name { get; private set; }
        public List<MyreKeyframeContent> Keyframes { get; private set; }

        public MyreClipContent(string name)
        {
            Name = name;
            Keyframes = new List<MyreKeyframeContent>();
        }
    }

    [ContentTypeWriter]
    public class MyreAnimationContentWriter : ContentTypeWriter<MyreClipContent>
    {
        protected override void Write(ContentWriter output, MyreClipContent value)
        {
            output.Write(value.Name);

            output.Write(value.Keyframes.Count);
            for (int i = 0; i < value.Keyframes.Count; i++)
                output.WriteObject(value.Keyframes[i]);
        }

        public override string GetRuntimeReader(TargetPlatform targetPlatform)
        {
            return "Myre.Graphics.Animation.ClipReader, Myre.Graphics";
        }

        public override string GetRuntimeType(TargetPlatform targetPlatform)
        {
            return "Myre.Graphics.Animation.Clip, Myre.Graphics";
        }
    }
    #endregion

    #region Keyframe
    [ContentSerializerRuntimeType("Myre.Graphics.Animation.Keyframe, Myre.Graphics")]
    public class MyreKeyframeContent
    {
        public int Bone { get; set; }
        public TimeSpan Time { get; set; }
        public Matrix Transform { get; set; }

        public MyreKeyframeContent(int boneIndex, TimeSpan timeSpan, Matrix matrix)
        {
            Bone = boneIndex;
            Time = timeSpan;
            Transform = matrix;
        }
    }

    [ContentTypeWriter]
    public class MyreKeyframeContentWriter : ContentTypeWriter<MyreKeyframeContent>
    {
        protected override void Write(ContentWriter output, MyreKeyframeContent value)
        {
            output.Write(value.Bone);
            output.Write(value.Time.Ticks);
            output.Write(value.Transform);
        }

        public override string GetRuntimeReader(TargetPlatform targetPlatform)
        {
            return "Myre.Graphics.Animation.KeyframeReader, Myre.Graphics";
        }

        public override string GetRuntimeType(TargetPlatform targetPlatform)
        {
            return "Myre.Graphics.Animation.Keyframe, Myre.Graphics";
        }
    }
    #endregion

    #region skinning data
    [ContentSerializerRuntimeType("Myre.Graphics.Animation.SkinningData, Myre.Graphics")]
    public class MyreSkinningDataContent
    {
        public List<Matrix> BindPose { get; private set; }
        public List<Matrix> InverseBindPose { get; private set; }
        public List<int> Hierarchy { get; private set; }

        public MyreSkinningDataContent(List<Matrix> bindPose, List<Matrix> inverseBindPose, List<int> hierarchy)
        {
            BindPose = bindPose;
            InverseBindPose = inverseBindPose;
            Hierarchy = hierarchy;
        }
    }

    [ContentTypeWriter]
    public class MyreSkinningDataContentWriter : ContentTypeWriter<MyreSkinningDataContent>
    {
        protected override void Write(ContentWriter output, MyreSkinningDataContent value)
        {
            //Write bind pose
            output.Write(value.BindPose.Count);
            foreach (var matrix in value.BindPose)
                output.Write(matrix);

            //Write inverse bind pose
            output.Write(value.InverseBindPose.Count);
            foreach (var matrix in value.InverseBindPose)
                output.Write(matrix);

            //Write skeleton hierarchy
            output.Write(value.Hierarchy.Count);
            foreach (var i in value.Hierarchy)
                output.Write(i);
        }

        public override string GetRuntimeReader(TargetPlatform targetPlatform)
        {
            return "Myre.Graphics.Animation.SkinningDataReader, Myre.Graphics";
        }

        public override string GetRuntimeType(TargetPlatform targetPlatform)
        {
            return "Myre.Graphics.Animation.SkinningData, Myre.Graphics";
        }
    }
    #endregion
}
