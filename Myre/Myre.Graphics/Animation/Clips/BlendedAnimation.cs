using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Myre.Graphics.Animation.Clips
{
    public class BlendedAnimation
        :IClip
    {
        public string Name { get; private set; }

        public BlendedAnimation(params KeyValuePair<IClip, float>[] blendClips)
        {
            throw new NotImplementedException();
        }

        public void Start()
        {
            throw new NotImplementedException();
        }

        public Keyframe[] Keyframes { get; private set; }
        public TimeSpan Duration { get; private set; }
    }
}
