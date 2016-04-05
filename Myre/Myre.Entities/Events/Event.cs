using System.Collections.Generic;
using System.Diagnostics.Contracts;
using Myre.Collections;

namespace Myre.Entities.Events
{
    /// <summary>
    /// A class which represents an event for a specified data type.
    /// Instances of this type can be used to send events to listeners which have registered with this event.
    /// </summary>
    /// <typeparam name="TData">The type of payload data this event sends.</typeparam>
    public class Event<TData>
        : IEvent
    {
        private class Invocation
            : IEventInvocation
        {
            public TData Data;
            public Event<TData> Event;

            public void Execute()
            {
                //Loop over event listeners backwards so most recent handlers are executed first
                //This makes events compatible with using them as a chained system where more recent handlers can temporarily block lower handlers (by modifying the event data)
                for (var i = Event._listeners.Count - 1; i >= 0; i--)
                    Data = Event._listeners[i].HandleEvent(Data, Event._scope);
            }


            public void Recycle()
            {
                Data = default(TData);
                Pool<Invocation>.Instance.Return(this);
            }
        }

        private readonly EventService _service;
        private readonly object _scope;
        private readonly Event<TData> _global;
        private readonly List<IEventListener<TData>> _listeners;

        /// <summary>
        /// Gets the service.
        /// </summary>
        /// <value>The service.</value>
        public EventService Service
        {
            get
            {
                Contract.Ensures(Contract.Result<EventService>() != null);
                return _service;
            }
        }

        internal Event(EventService service, object scope = null, Event<TData> globalScoped = null)
        {
            Contract.Requires(service != null);

            _service = service;
            _scope = scope;
            _global = globalScoped;
            _listeners = new List<IEventListener<TData>>();
        }

        /// <summary>
        /// Adds a listener.
        /// </summary>
        /// <param name="listener">The listener.</param>
        public void AddListener(IEventListener<TData> listener)
        {
            _listeners.Add(listener);
        }

        /// <summary>
        /// Removes a listener.
        /// </summary>
        /// <param name="listener">The listener.</param>
        /// <returns></returns>
        public bool RemoveListener(IEventListener<TData> listener)
        {
            return _listeners.Remove(listener);
        }

        /// <summary>
        /// Sends the specified data to all registered listeners.
        /// </summary>
        /// <param name="data">The data.</param>
        public void Send(TData data)
        {
            Send(data, this);

            if (_global != null)
                Send(data, _global);
        }

        private void Send(TData data, Event<TData> channel)
        {
            Invocation invocation = Pool<Invocation>.Instance.Get();
            invocation.Event = channel;
            invocation.Data = data;

            _service.Queue(invocation);
        }
    }
}
