using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

namespace Myre.Graphics.Animation
{
    [Serializable]
    public class Keyframe
    {
        public int Bone { get; set; }
        public TimeSpan Time { get; set; }

        public Transform Transform { get; set; }

        internal Keyframe()
        {
        }

        public Keyframe(int bone, TimeSpan time, Vector3 position, Vector3 scale, Quaternion orientaton)
        {
            Bone = bone;
            Time = time;

            Transform = new Transform
            {
                Translation = position,
                Scale = scale,
                Rotation = orientaton,
            };
        }

        public override string ToString()
        {
            return string.Format("bone {0} @ time {1}", Bone, Time);
        }
    }

    public class KeyframeReader : ContentTypeReader<Keyframe>
    {
        protected override Keyframe Read(ContentReader input, Keyframe existingInstance)
        {
            existingInstance = existingInstance ?? new Keyframe();

            existingInstance.Bone = input.ReadInt32();
            existingInstance.Time = new TimeSpan(input.ReadInt64());

            existingInstance.Transform = new Transform
            {
                Translation = input.ReadVector3(),
                Scale = input.ReadVector3(),
                Rotation = input.ReadQuaternion()
            };

            return existingInstance;
        }
    }
}
