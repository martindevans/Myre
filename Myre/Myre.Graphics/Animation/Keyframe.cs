using System;
using System.Numerics;
using Microsoft.Xna.Framework.Content;
using Myre.Extensions;

namespace Myre.Graphics.Animation
{
    [Serializable]
    public struct Keyframe
    {
        public readonly ushort Bone;
        public readonly TimeSpan Time;
        public readonly Transform Transform;

        public Keyframe(ushort bone, TimeSpan time, Vector3 position, Vector3 scale, Quaternion orientaton)
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
            return new Keyframe(input.ReadUInt16(), new TimeSpan(input.ReadInt64()), input.ReadVector3().FromXNA(), input.ReadVector3().FromXNA(), input.ReadQuaternion().FromXNA());
        }
    }
}
