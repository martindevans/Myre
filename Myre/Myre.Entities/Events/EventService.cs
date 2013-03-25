using System;
using System.Collections.Generic;
using Myre.Entities.Services;

namespace Myre.Entities.Events
{
    interface IEventInvocation
        : IRecycleable
    {
        void Execute();
    }

    /// <summary>
    /// An interface which defines methods for retrieving events.
    /// </summary>
    public interface IEventService
        : IService
    {
// ReSharper disable UnusedMemberInSuper.Global
        Event<T> GetEvent<T>(object scope = null);
// ReSharper restore UnusedMemberInSuper.Global
    }

    /// <summary>
    /// A class which manages the sending of events.
    /// </summary>
    public class EventService
        : Service, IEventService
    {
        class Events
        {
            public object GlobalScope;
            public readonly Dictionary<object, object> LocalScopes = new Dictionary<object, object>();
        }

        private readonly Dictionary<Type, Events> _events;
        private Queue<IEventInvocation> _waitingEvents;
        private Queue<IEventInvocation> _executingEvents;
        private SpinLock _spinLock;

        /// <summary>
        /// Initializes a new instance of the <see cref="EventService"/> class.
        /// </summary>
        public EventService()
        {
            _spinLock = new SpinLock();
            _events = new Dictionary<Type, Events>();
            _waitingEvents = new Queue<IEventInvocation>();
            _executingEvents = new Queue<IEventInvocation>();
        }

        /// <summary>
        /// Gets an event of the specified type.
        /// </summary>
        /// <typeparam name="T">The type of data this event sends.</typeparam>
        /// <returns></returns>
        public Event<T> GetEvent<T>(object scope = null)
        {
            var type = typeof(T);

            Events e;
            if (!_events.TryGetValue(type, out e))
            {
                e = new Events {GlobalScope = new Event<T>(this)};
                _events[type] = e;
            }

            object instance;
            if (scope == null)
                instance = e.GlobalScope;
            else
            {
                if (!e.LocalScopes.TryGetValue(scope, out instance))
                {
                    instance = new Event<T>(this, scope, e.GlobalScope as Event<T>);
                    e.LocalScopes[scope] = instance;
                }
            }

            return instance as Event<T>;
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
            try
            {
                _spinLock.Lock();
                _waitingEvents.Enqueue(eventInvocation);
            }
            finally
            {
                _spinLock.Unlock();
            }
        }

        private void ExecuteEvents()
        {
            while (_executingEvents.Count > 0)
            {
                IEventInvocation invocation = _executingEvents.Dequeue();
                invocation.Execute();
                invocation.Recycle();
            }
        }

        private void FlipBuffers()
        {
            try
            {
                _spinLock.Lock();
                var tmp = _waitingEvents;
                _waitingEvents = _executingEvents;
                _executingEvents = tmp;
            }
            finally
            {
                _spinLock.Unlock();
            }
        }
    }
}
