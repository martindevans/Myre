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

        public Vector3 Position { get; set; }
        public Vector3 Scale { get; set; }
        public Quaternion Orientation { get; set; }

        internal Keyframe()
        {
        }

        public Keyframe(int bone, TimeSpan time, Vector3 position, Vector3 scale, Quaternion orientaton)
        {
            Bone = bone;
            Time = time;

            Position = position;
            Scale = scale;
            Orientation = orientaton;
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

            existingInstance.Position = input.ReadVector3();
            existingInstance.Scale = input.ReadVector3();
            existingInstance.Orientation = input.ReadQuaternion();

            return existingInstance;
        }
    }
}
