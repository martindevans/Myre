using System;
using System.Collections.Generic;
using System.Numerics;

namespace Myre.Graphics.Animation
{
    public static class AnimationHelpers
    {
        /// <summary>
        /// Calculate bone transforms from world transforms
        /// </summary>
        /// <param name="hierarchy">Hierarchy of the skeleton, value for a given index indicates the parent index of the bone at the given index</param>
        /// <param name="worldTransforms">The world transforms to turn into bone transforms</param>
        /// <param name="calculatedBoneTransforms">An array to write the bone transforms into</param>
        public static void CalculateBoneTransformsFromWorldTransforms(IList<int> hierarchy, IReadOnlyList<Matrix4x4> worldTransforms, Matrix4x4[] calculatedBoneTransforms)
        {
            if (calculatedBoneTransforms == null)
                throw new ArgumentNullException("calculatedBoneTransforms");
            if (worldTransforms == null)
                throw new ArgumentNullException("worldTransforms");

            unsafe
            {
                //Allocate a place to temporarily store the inverted transforms
                Matrix4x4* inverseWorldTransforms = stackalloc Matrix4x4[worldTransforms.Count];

                //Calculate inverse world transforms for each bone
                for (var i = 0; i < calculatedBoneTransforms.Length; i++)
                    Matrix4x4.Invert(worldTransforms[i], out inverseWorldTransforms[i]);

                //Calculate bone transforms for each bone
                for (var bone = 0; bone < worldTransforms.Count; bone++)
                {
                    var parentBone = hierarchy[bone];
                    if (parentBone == -1)
                        calculatedBoneTransforms[bone] = worldTransforms[bone];
                    else
                        calculatedBoneTransforms[bone] = Matrix4x4.Multiply(worldTransforms[bone], inverseWorldTransforms[parentBone]);
                }
            }
        }

        /// <summary>
        /// Calculate world transforms down bone hierarchy
        /// </summary>
        /// <param name="hierarchy"></param>
        /// <param name="boneTransforms"></param>
        /// <param name="calculatedWorldTransforms"></param>
        public static void CalculateWorldTransformsFromBoneTransforms(IList<int> hierarchy, IReadOnlyList<Matrix4x4> boneTransforms, Matrix4x4[] calculatedWorldTransforms)
        {
            if (boneTransforms == null)
                throw new ArgumentNullException("boneTransforms");
            if (calculatedWorldTransforms == null)
                throw new ArgumentNullException("calculatedWorldTransforms");

            // Root bone transform is just the given transform
            calculatedWorldTransforms[0] = boneTransforms[0];

            // Child world transform is bone_transform x parent_world_transform
            for (var bone = 1; bone < boneTransforms.Count; bone++)
            {
                var parentBone = hierarchy[bone];

                //Multiply by parent bone transform
                calculatedWorldTransforms[bone] = Matrix4x4.Multiply(boneTransforms[bone], calculatedWorldTransforms[parentBone]);
            }
        }
    }
}
