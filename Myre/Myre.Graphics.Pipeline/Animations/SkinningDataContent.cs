using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;

namespace Myre.Graphics.Pipeline.Animations
{
    [ContentSerializerRuntimeType("Myre.Graphics.Animation.SkinningData, Myre.Graphics")]
    public class SkinningDataContent
    {
        public List<Matrix> BindPose { get; private set; }
        public List<Matrix> InverseBindPose { get; private set; }
        public List<int> Hierarchy { get; private set; }

        public SkinningDataContent(List<Matrix> bindPose, List<Matrix> inverseBindPose, List<int> hierarchy)
        {
            BindPose = bindPose;
            InverseBindPose = inverseBindPose;
            Hierarchy = hierarchy;
        }
    }

    [ContentTypeWriter]
    public class SkinningDataContentWriter : ContentTypeWriter<SkinningDataContent>
    {
        protected override void Write(ContentWriter output, SkinningDataContent value)
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
}
