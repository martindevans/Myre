using System.Threading;
using Myre.Entities.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace Myre.Entities.Events
{
    interface IEventInvocation
    {
        void Execute();

        void Recycle();
    }

    interface IEvent
    {

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
            public IEvent GlobalScope;
            public readonly Dictionary<object, IEvent> LocalScopes = new Dictionary<object, IEvent>();
        }

        private readonly Dictionary<Type, Events> _events = new Dictionary<Type, Events>();
        private Queue<IEventInvocation> _waitingEvents = new Queue<IEventInvocation>();
        private Queue<IEventInvocation> _executingEvents = new Queue<IEventInvocation>();
        private SpinLock _spinLock = new SpinLock();

        [ContractInvariantMethod]
        private void ObjectInvariant()
        {
            Contract.Invariant(_waitingEvents != null);
            Contract.Invariant(_executingEvents != null);
        }

        /// <summary>
        /// Gets an event of the specified type.
        /// </summary>
        /// <typeparam name="T">The type of data this event sends.</typeparam>
        /// <param name="scope">The scope of this event. Messages sent to an unscoped event are only received by the unscoped event, messages sent to a scoped event are only sent to listeners with the same scope *and* to the unscoped event</param>
        /// <returns></returns>
        public Event<T> GetEvent<T>(object scope = null)
        {
            var type = typeof(T);

            Events e;
            if (!_events.TryGetValue(type, out e))
            {
                e = new Events { GlobalScope = new Event<T>(this) };
                _events[type] = e;
            }

            Contract.Assume(e != null);

            IEvent instance;
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
            bool taken = false;
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
                Contract.Assume(invocation != null);

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
