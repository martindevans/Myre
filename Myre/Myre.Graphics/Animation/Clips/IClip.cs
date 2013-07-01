using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Myre.Graphics.Animation.Clips
{
    public interface IClip
    {
        /// <summary>
        /// The name of this animation
        /// </summary>
        string Name { get; }

        /// <summary>
        /// This animation is about to start playing
        /// if this aniamtion is set to loop, this will be called every time a iteration loop starts
        /// </summary>
        void Start();

        /// <summary>
        /// The keyframes of this animation in time order
        /// </summary>
        Keyframe[] Keyframes { get; }

        /// <summary>
        /// The duration of this animation
        /// </summary>
        TimeSpan Duration { get; }
    }
}
