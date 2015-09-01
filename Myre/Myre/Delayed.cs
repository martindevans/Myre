using System;
using System.Collections.Generic;

using MathHelper = Microsoft.Xna.Framework.MathHelper;

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
            lock (_buffer)
            {
                _buffer.Add(new Event {
                    Duration = delay,
                    Completed = action
                });
            }
        }

        /// <summary>
        /// Calls the specified delegate every frame for the specified number of seconds, and then calls the specified callback delegate.
        /// </summary>
        /// <param name="step">The method to call each frame. This method takes on float parameter which is the progress from 0 to 1.</param>
        /// <param name="completionCallback">The method to call on completion of the transition.</param>
        /// <param name="duration">The number of seconds to call the delegate for.</param>
        public static void Transition(Action<float> step, TimeSpan duration, Action completionCallback = null)
        {
            lock (_buffer)
            {
                _buffer.Add(new Event() {
                    Duration = (float)duration.TotalSeconds,
                    Completed = completionCallback,
                    Transition = step
                });
            }
        }

        /// <summary>
        /// Updates all transitions.
        /// </summary>
        /// <param name="gameTime"></param>
        public static void Update(Microsoft.Xna.Framework.GameTime gameTime)
        {
            //Move items from add buffer to events, keep lock as small as possible
            lock (_buffer)
            {
                //Copy events from buffer to event collection, initialise the start time
                for (int i = 0; i < _buffer.Count; i++)
                {
                    var e = _buffer[i];
                    e.Start = gameTime.TotalGameTime.TotalSeconds;

                    _events.Add(e);
                }

                _buffer.Clear();
            }

            //Step all event
            for (int i = _events.Count - 1; i >= 0; i--)
            {
                //Update progress
                var e = _events[i];
                e.Progress = MathHelper.Clamp((float)(gameTime.TotalGameTime.TotalSeconds - e.Start) / e.Duration, 0, 1);

                //Call per tick update (if applicable)
                if (e.Transition != null)
                    e.Transition(e.Progress);

                //Remove finished events
                if (e.Progress >= 1)
                {
                    if (e.Completed != null)
                        e.Completed();

                    _events.RemoveAt(i);
                }
                else
                {
                    //We've been mutating a local copy
                    _events[i] = e;
                }
            }
        }
    }

    struct Event
    {
        public double Start;
        public float Duration;
        public float Progress;
        public Action Completed;
        public Action<float> Transition;
    }
}
