using System;
using System.Numerics;
using Microsoft.Xna.Framework.Content;
using Myre.Extensions;

namespace Myre.Graphics.Animation
{
    [Serializable]
    public class SkinningData
    {
        /// <summary>
        /// Bindpose matrices for each bone in the skeleton,
        /// relative to the parent bone.
        /// </summary>
        public Matrix4x4[] BindPose { get; internal set; }

        /// <summary>
        /// Vertex to bonespace transforms for each bone in the skeleton.
        /// </summary>
        public Matrix4x4[] InverseBindPose { get; internal set; }

        /// <summary>
        /// For each bone in the skeleton, stores the index of the parent bone.
        /// </summary>
        public int[] SkeletonHierarchy { get; internal set; }

        /// <summary>
        /// Names of the bones
        /// </summary>
        public string[] Names { get; internal set; }

        /// <summary>
        /// Bounding matrices for each bone. Aligned with the bone
        /// </summary>
        public BoundingBox[] Bounds { get; internal set; }
    }

    public class SkinningDataReader : ContentTypeReader<SkinningData>
    {
        protected override SkinningData Read(ContentReader input, SkinningData existingInstance)
        {
            existingInstance = existingInstance ?? new SkinningData();

            //Read bind pose
            existingInstance.BindPose = new Matrix4x4[input.ReadInt32()];
            for (int i = 0; i < existingInstance.BindPose.Length; i++)
                existingInstance.BindPose[i] = new Transform { Translation = input.ReadVector3().FromXNA(), Scale = input.ReadVector3().FromXNA(), Rotation = input.ReadQuaternion().FromXNA() }.ToMatrix();

            //Read inverse bind pose
            existingInstance.InverseBindPose = new Matrix4x4[input.ReadInt32()];
            for (int i = 0; i < existingInstance.InverseBindPose.Length; i++)
                existingInstance.InverseBindPose[i] = input.ReadMatrix().FromXNA();

            //Read skeleton hierarchy
            existingInstance.SkeletonHierarchy = new int[input.ReadInt32()];
            for (int i = 0; i < existingInstance.SkeletonHierarchy.Length; i++)
                existingInstance.SkeletonHierarchy[i] = input.ReadInt32();

            //Read names
            existingInstance.Names = new string[input.ReadInt32()];
            for (int i = 0; i < existingInstance.Names.Length; i++)
                existingInstance.Names[i] = input.ReadString();

            //Read per bone bounding boxes
            existingInstance.Bounds = new BoundingBox[input.ReadInt32()];
            for (int i = 0; i < existingInstance.Bounds.Length; i++)
                existingInstance.Bounds[i] = input.ReadObject<Microsoft.Xna.Framework.BoundingBox>().FromXNA();

            return existingInstance;
        }
    }
}
