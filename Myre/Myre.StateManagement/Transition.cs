using System;
using Myre.Extensions;

using GameTime = Microsoft.Xna.Framework.GameTime;
using MathHelper = Microsoft.Xna.Framework.MathHelper;

namespace Myre.StateManagement
{
    /// <summary>
    /// A class which represents a transition.
    /// </summary>
    public class Transition
    {
        private TimeSpan _onTime;
        private TimeSpan _offTime;
        private float _progress;
        private bool _targettingOn;

        /// <summary>
        /// The time taken to transition from 0 to 1.
        /// </summary>
        public TimeSpan OnDuration
        {
            get { return _onTime; }
            set
            {
                if (value.Ticks < 0)
                    throw new ArgumentOutOfRangeException("value", "Cannot be less than 0.");
                _onTime = value;
            }
        }

        /// <summary>
        /// The time taken to transition from 1 to 0.
        /// </summary>
        public TimeSpan OffDuration
        {
            get { return _offTime; }
            set
            {
                if (value.Ticks < 0)
                    throw new ArgumentOutOfRangeException("value", "Cannot be less than 0.");
                _offTime = value;
            }
        }

        /// <summary>
        /// The state of the transition. Between 0 (off) and 1 (on).
        /// </summary>
        public float Progress
        {
            get { return _progress; }
        }

        /// <summary>
        /// Creates a new instance of the Transition class.
        /// </summary>
        /// <param name="onDuration">The time taken to transition from 0 to 1.</param>
        /// <param name="offDuration">The time taken to transition from 1 to 0.</param>
        public Transition(TimeSpan onDuration, TimeSpan offDuration)
        {
            _onTime = onDuration;
            _offTime = offDuration;
        }

        /// <summary>
        ///  Creates a new instance of the Transition class.
        /// </summary>
        /// <param name="duration">The time taken to transition on or off.</param>
        public Transition(TimeSpan duration)
            : this(duration, duration)
        {
        }

        /// <summary>
        /// Updates the transition.
        /// </summary>
        /// <param name="gameTime">Game time.</param>
        public void Update(GameTime gameTime)
        {
            var dt = gameTime.Seconds();
            if (_targettingOn)
            {
                if (_onTime.Ticks == 0)
                    _progress = 1;
                else
                    _progress = MathHelper.Clamp(_progress + (dt / (float)_onTime.TotalSeconds), 0, 1);
            }
            else
            {
                if (_offTime.Ticks == 0)
                    _progress = 0;
                else
                    _progress = MathHelper.Clamp(_progress - (dt / (float)_offTime.TotalSeconds), 0, 1);
            }
        }

        /// <summary>
        /// Makes the transition move towards 1.
        /// </summary>
        public void MoveOn()
        {
            _targettingOn = true;
        }

        /// <summary>
        /// Makes the transition move towards 0.
        /// </summary>
        public void MoveOff()
        {
            _targettingOn = false;
        }
    }
}
