using System.Threading;
using Myre.Entities.Services;
using System;
using System.Collections.Generic;

namespace Myre.Entities.Events
{
    internal interface IEventInvocation
    {
        void Execute();

        void Recycle();
    }

    internal interface IEvent
    {

    }

    /// <summary>
    /// An interface which defines methods for retrieving events.
    /// </summary>
    public interface IEventService
        : IService
    {
        Event<T> GetEvent<T>();
    }

    /// <summary>
    /// A class which manages the sending of events.
    /// </summary>
    public class EventService
        : Service, IEventService
    {
        private readonly Dictionary<Type, IEvent> _events = new();
        private Queue<IEventInvocation> _waitingEvents = new();
        private Queue<IEventInvocation> _executingEvents = new();
        private SpinLock _spinLock;

        /// <summary>
        /// Gets an event of the specified type.
        /// </summary>
        /// <typeparam name="T">The type of data this event sends.</typeparam>
        /// <returns></returns>
        public Event<T> GetEvent<T>()
        {
            var type = typeof(T);

            if (!_events.TryGetValue(type, out var e))
            {
                e = new Event<T>(this);
                _events[type] = e;
            }

            return (Event<T>)e;
        }

        /// <summary>
        /// Sends any queued events.
        /// </summary>
        /// <param name="elapsedTime">The elapsed time.</param>
        public override void Update(float elapsedTime)
        {
            while (_waitingEvents.Count > 0)
            {
                FlipBuffers();
                ExecuteEvents();
            }
        }

        internal void Queue(IEventInvocation eventInvocation)
        {
            var taken = false;
            try
            {
                _spinLock.Enter(ref taken);
                _waitingEvents.Enqueue(eventInvocation);
            }
            finally
            {
                if (taken)
                    _spinLock.Exit();
            }
        }

        private void ExecuteEvents()
        {
            while (_executingEvents.Count > 0)
            {
                var invocation = _executingEvents.Dequeue();

                invocation.Execute();
                invocation.Recycle();
            }
        }

        private void FlipBuffers()
        {
            var taken = false;
            try
            {
                _spinLock.Enter(ref taken);
                var tmp = _waitingEvents;
                _waitingEvents = _executingEvents;
                _executingEvents = tmp;
            }
            finally
            {
                if (taken)
                    _spinLock.Exit();
            }
        }
    }
}
