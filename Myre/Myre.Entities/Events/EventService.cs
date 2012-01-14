using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Myre.Entities.Services;
using Myre;

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
        Event<T> GetEvent<T>(object scope = null);
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
            public Dictionary<object, object> LocalScopes = new Dictionary<object, object>();
        }

        private Dictionary<Type, Events> events;
        private Queue<IEventInvocation> waitingEvents;
        private Queue<IEventInvocation> executingEvents;
        private SpinLock spinLock;

        /// <summary>
        /// Initializes a new instance of the <see cref="EventService"/> class.
        /// </summary>
        public EventService()
        {
            events = new Dictionary<Type, Events>();
            waitingEvents = new Queue<IEventInvocation>();
            executingEvents = new Queue<IEventInvocation>();
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
            if (!events.TryGetValue(type, out e))
            {
                e = new Events();
                e.GlobalScope = new Event<T>(this);
                events[type] = e;
            }

            object instance = null;
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
            while (waitingEvents.Count > 0)
            {
                FlipBuffers();
                ExecuteEvents();
            }
        }

        internal void Queue(IEventInvocation eventInvocation)
        {
            try
            {
                spinLock.Lock();
                waitingEvents.Enqueue(eventInvocation);
            }
            finally
            {
                spinLock.Unlock();
            }
        }

        private void ExecuteEvents()
        {
            while (executingEvents.Count > 0)
            {
                IEventInvocation invocation = executingEvents.Dequeue();
                invocation.Execute();
                invocation.Recycle();
            }
        }

        private void FlipBuffers()
        {
            try
            {
                spinLock.Lock();
                var tmp = waitingEvents;
                waitingEvents = executingEvents;
                executingEvents = tmp;
            }
            finally
            {
                spinLock.Unlock();
            }
        }
    }
}
