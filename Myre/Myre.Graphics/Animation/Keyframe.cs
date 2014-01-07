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

        public Vector3 Translation { get; set; }
        public Vector3 Scale { get; set; }
        public Quaternion Rotation { get; set; }

        internal Keyframe()
        {
        }

        public Keyframe(int bone, TimeSpan time, Vector3 position, Vector3 scale, Quaternion orientaton)
        {
            Bone = bone;
            Time = time;

            Translation = position;
            Scale = scale;
            Rotation = orientaton;
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

            existingInstance.Translation = input.ReadVector3();
            existingInstance.Scale = input.ReadVector3();
            existingInstance.Rotation = input.ReadQuaternion();

            return existingInstance;
        }
    }
}
