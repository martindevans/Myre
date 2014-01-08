﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

namespace Myre.Graphics.Animation
{
    public class SkinningData
    {
        /// <summary>
        /// Bindpose matrices for each bone in the skeleton,
        /// relative to the parent bone.
        /// </summary>
        public Transform[] BindPose { get; internal set; }

        /// <summary>
        /// Vertex to bonespace transforms for each bone in the skeleton.
        /// </summary>
        public Matrix[] InverseBindPose { get; internal set; }

        /// <summary>
        /// For each bone in the skeleton, stores the index of the parent bone.
        /// </summary>
        public int[] SkeletonHierarchy { get; internal set; }

        /// <summary>
        /// Names of the bones
        /// </summary>
        public string[] Names { get; internal set; }
    }

    public class SkinningDataReader : ContentTypeReader<SkinningData>
    {
        protected override SkinningData Read(ContentReader input, SkinningData existingInstance)
        {
            existingInstance = existingInstance ?? new SkinningData();

            //Read bind pose
            existingInstance.BindPose = new Transform[input.ReadInt32()];
            for (int i = 0; i < existingInstance.BindPose.Length; i++)
                existingInstance.BindPose[i] = new Transform { Translation = input.ReadVector3(), Scale = input.ReadVector3(), Rotation = input.ReadQuaternion() };

            //Read inverse bind pose
            existingInstance.InverseBindPose = new Matrix[input.ReadInt32()];
            for (int i = 0; i < existingInstance.InverseBindPose.Length; i++)
                existingInstance.InverseBindPose[i] = input.ReadMatrix();

            //Read skeleton hierarchy
            existingInstance.SkeletonHierarchy = new int[input.ReadInt32()];
            for (int i = 0; i < existingInstance.SkeletonHierarchy.Length; i++)
                existingInstance.SkeletonHierarchy[i] = input.ReadInt32();

            existingInstance.Names = new string[input.ReadInt32()];
            for (int i = 0; i < existingInstance.Names.Length; i++)
                existingInstance.Names[i] = input.ReadString();

            return existingInstance;
        }
    }
}
