using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Myre
{
    /// <summary>
    /// A static class containing methods for fire-and-forget delayed events and transitions.
    /// </summary>
    public static class Delayed
    {
        static readonly List<Event> _events = new List<Event>();
        static readonly List<Event> _buffer = new List<Event>();

        /// <summary>
        /// Fires the spevified action after the specified number of seconds have elapsed.
        /// </summary>
        /// <param name="action">The delegate to execute.</param>
        /// <param name="delay">The delay before the action is executed.</param>
        public static void Action(Action action, float delay)
        {
            _buffer.Add(new Event
            {
                Start = DateTime.Now,
                Duration = delay,
                Completed = action
            });
        }

        /// <summary>
        /// Calls the specified delegate every frame for the specified number of seconds, and then calls the specified callback delegate.
        /// </summary>
        /// <param name="step">The method to call each frame. This method takes on float parameter which is the progress from 0 to 1.</param>
        /// <param name="completionCallback">The method to call on completion of the transition.</param>
        /// <param name="duration">The number of seconds to call the delegate for.</param>
        public static void Transition(Action<float> step, float duration, Action completionCallback = null)
        {
            _buffer.Add(new Event()
            {
                Start = DateTime.Now,
                Duration = duration,
                Completed = completionCallback,
                Transition = step
            });
        }

        /// <summary>
        /// Updates all transitions. This is called by MyreGame.Update(gameTime).
        /// </summary>
        /// <param name="gameTime"></param>
        public static void Update(GameTime gameTime)
        {
            var now = DateTime.Now;

            _events.AddRange(_buffer);
            _buffer.Clear();

            for (int i = _events.Count - 1; i >= 0; i--)
            {
                var e = _events[i];
                e.Progress = MathHelper.Clamp((float)(now - e.Start).TotalSeconds / e.Duration, 0, 1);

                if (e.Transition != null)
                    e.Transition(e.Progress);

// ReSharper disable CompareOfFloatsByEqualityOperator
                if (e.Progress == 1)
// ReSharper restore CompareOfFloatsByEqualityOperator
                {
                    if (e.Completed != null)
                        e.Completed();

                    _events.RemoveAt(i);
                }
                else
                    _events[i] = e;
            }
        }
    }

    struct Event
    {
        public DateTime Start;
        public float Duration;
        public float Progress;
        public Action Completed;
        public Action<float> Transition;
    }
}
