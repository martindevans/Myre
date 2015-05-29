using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;

namespace Myre.Graphics.Pipeline.Animations
{
    [ContentProcessor(DisplayName = "Animation Processor")]
    public class AnimationProcessor
        : BaseAnimationProcessor<NodeContent>
    {
        private string _takeName;

        [DisplayName("Take Name")]
        [DefaultValue(null)]
        public string TakeName
        {
            get
            {
                return _takeName;
            }
            set
            {
                _takeName = value;
            }
        }

        private string _rootBone;
        [DisplayName("Root Bone")]
        [DefaultValue(null)]
        public string RootBone
        {
            get
            {
                return _rootBone;
            }
            set
            {
                _rootBone = value;
            }
        }

        private bool _fixLooping;
        [DisplayName("Fix Looping")]
        [DefaultValue(false)]
        // ReSharper disable once ConvertToAutoProperty
        public bool FixLooping
        {
            get
            {
                return _fixLooping;
            }
            set
            {
                _fixLooping = value;
            }
        }

        private bool _linearKeyframeReduction;
        [DisplayName("Linear Keyframe Reduction")]
        [DefaultValue(false)]
        // ReSharper disable once ConvertToAutoProperty
        public bool LinearKeyframeReduction
        {
            get
            {
                return _linearKeyframeReduction;
            }
            set
            {
                _linearKeyframeReduction = value;
            }
        }

        private string _restPose;
        [DisplayName("Rest Pose")]
        [DefaultValue(null)]
        public string RestPose
        {
            get { return _restPose; }
            set { _restPose = value; }
        }

        public override ClipContent Process(NodeContent input, ContentProcessorContext context)
        {
            var animations = FindAnimations(input).ToDictionary(a => a.Key, a => a.Value);

            //Find the appropriate animation
            AnimationContent animation = null;
            if (string.IsNullOrWhiteSpace(_takeName))
            {
                //No animation specified, take the only one in the file
                var anims = animations.ToArray();
                if (anims.Length == 1)
                    animation = anims[0].Value;
                else
                    throw new InvalidOperationException(string.Format("Expected single animation, found {0}", animations.Select(a => a.Key).Aggregate((a, b) => a + "," + b)));
            }
            else
            {
                //Take the specified animation
                animations.TryGetValue(TakeName, out animation);
            }

            //Failed to find an appropriate animation :(
            if (animation == null)
                throw new KeyNotFoundException(string.Format(@"Animation '{0}' not found, only options are {1}", TakeName, animations.Select(a => a.Key).Aggregate((a, b) => a + "," + b)));

            //Load the rest pose
            IList<BoneContent> restPose = null;
            if (RestPose != null)
                restPose = MeshHelper.FlattenSkeleton(MeshHelper.FindSkeleton(context.BuildAndLoadAsset<NodeContent, NodeContent>(new ExternalReference<NodeContent>(RestPose), null)));

            return Process(input, animation, _rootBone, float.NegativeInfinity, float.PositiveInfinity, FixLooping, LinearKeyframeReduction, restPose);
        }
    }
}
