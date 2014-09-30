using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

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
        public static void CalculateBoneTransformsFromWorldTransforms(IList<int> hierarchy, Matrix[] worldTransforms, Matrix[] calculatedBoneTransforms)
        {
            //Calculate inverse world transforms for each bone
            Matrix[] inverseWorld = new Matrix[worldTransforms.Length];
            for (int i = 0; i < calculatedBoneTransforms.Length; i++)
                Matrix.Invert(ref worldTransforms[i], out inverseWorld[i]);

            //Calculate bone transforms for each bone
            for (int bone = 0; bone < worldTransforms.Length; bone++)
            {
                int parentBone = hierarchy[bone];
                if (parentBone == -1)
                    calculatedBoneTransforms[bone] = worldTransforms[bone];
                else
                    Matrix.Multiply(ref worldTransforms[bone], ref inverseWorld[parentBone], out calculatedBoneTransforms[bone]);
            }
        }

        /// <summary>
        /// Calculate world transforms down bone hierarchy
        /// </summary>
        /// <param name="hierarchy"></param>
        /// <param name="boneTransforms"></param>
        /// <param name="calculateWorldTransforms"></param>
        public static void CalculateWorldTransformsFromBoneTransforms(IList<int> hierarchy, Matrix[] boneTransforms, Matrix[] calculateWorldTransforms)
        {
            // Root bone.
            calculateWorldTransforms[0] = boneTransforms[0];

            // Child bones.
            for (int bone = 1; bone < boneTransforms.Length; bone++)
            {
                int parentBone = hierarchy[bone];

                //Multiply by parent bone transform
                Matrix.Multiply(ref boneTransforms[bone], ref calculateWorldTransforms[parentBone], out calculateWorldTransforms[bone]);
            }
        }
    }
}
