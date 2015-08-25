using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;
using System.Collections.Generic;

namespace Myre.Graphics.Pipeline.Animations
{
    [ContentSerializerRuntimeType("Myre.Graphics.Animation.SkinningData, Myre.Graphics")]
    public class SkinningDataContent
    {
        public List<Matrix> BindPose { get; private set; }
        public List<Matrix> InverseBindPose { get; private set; }
        public List<int> Hierarchy { get; private set; }
        public List<string> BoneNames { get; private set; }
        public List<Microsoft.Xna.Framework.BoundingBox> Bounds { get; private set; }

        public SkinningDataContent(List<Matrix> bindPose, List<Matrix> inverseBindPose, List<int> hierarchy, List<string> names, List<Microsoft.Xna.Framework.BoundingBox> bounds)
        {
            BindPose = bindPose;
            InverseBindPose = inverseBindPose;
            Hierarchy = hierarchy;
            BoneNames = names;
            Bounds = bounds;
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
            {
                Vector3 trans, scale;
                Quaternion rot;
                matrix.Decompose(out scale, out rot, out trans);

                output.Write(trans);
                output.Write(scale);
                output.Write(rot);
            }

            //Write inverse bind pose
            output.Write(value.InverseBindPose.Count);
            foreach (var matrix in value.InverseBindPose)
                output.Write(matrix);

            //Write skeleton hierarchy
            output.Write(value.Hierarchy.Count);
            foreach (var i in value.Hierarchy)
                output.Write(i);

            //Write names
            output.Write(value.BoneNames.Count);
            foreach (var boneName in value.BoneNames)
                output.Write(boneName);

            //Write bounds
            output.Write(value.Bounds.Count);
            foreach (var bound in value.Bounds)
                output.WriteObject<Microsoft.Xna.Framework.BoundingBox>(bound);
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
