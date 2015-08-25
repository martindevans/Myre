using System.Collections.Generic;
using System.Numerics;

namespace Myre.Graphics.Animation
{
    public static class AnimationHelpers
    {
        /// <summary>
        /// Calculate bone transforms from world transforms
        /// </summary>
        /// <param name="hierarchy"></param>
        /// <param name="worldTransforms"></param>
        /// <param name="calculatedBoneTransforms"></param>
        public static void CalculateBoneTransformsFromWorldTransforms(IList<int> hierarchy, Matrix4x4[] worldTransforms, Matrix4x4[] calculatedBoneTransforms)
        {
            unsafe
            {
                //Allocate a place to store the inverted transforms (on the stack to save allocations)
                Matrix4x4* inverseWorldTransforms = stackalloc Matrix4x4[worldTransforms.Length];

                //Calculate inverse world transforms for each bone
                for (int i = 0; i < calculatedBoneTransforms.Length; i++)
                    Matrix4x4.Invert(worldTransforms[i], out inverseWorldTransforms[i]);

                //Calculate bone transforms for each bone
                for (int bone = 0; bone < worldTransforms.Length; bone++)
                {
                    int parentBone = hierarchy[bone];
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
        /// <param name="calculateWorldTransforms"></param>
        public static void CalculateWorldTransformsFromBoneTransforms(IList<int> hierarchy, Matrix4x4[] boneTransforms, Matrix4x4[] calculateWorldTransforms)
        {
            // Root bone.
            calculateWorldTransforms[0] = boneTransforms[0];

            // Child bones.
            for (int bone = 1; bone < boneTransforms.Length; bone++)
            {
                int parentBone = hierarchy[bone];

                //Multiply by parent bone transform
                calculateWorldTransforms[bone] = Matrix4x4.Multiply(boneTransforms[bone], calculateWorldTransforms[parentBone]);
            }
        }
    }
}
