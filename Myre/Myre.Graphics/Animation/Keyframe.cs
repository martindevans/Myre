using System;
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

        public Keyframe(ushort bone, TimeSpan time, Transform transform)
        {
            Bone = bone;
            Time = time;
            Transform = transform;
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
            return new Keyframe(
                input.ReadUInt16(),
                new TimeSpan(input.ReadInt64()),
                new Transform {
                    Translation = input.ReadVector3().FromXNA(),
                    Scale = input.ReadVector3().FromXNA(),
                    Rotation = input.ReadQuaternion().FromXNA()
                }
            );
        }
    }
}
