using System.Collections.Generic;

namespace Myre.Entities.Events
{
    /// <summary>
    /// A delegate for handling events through the Event.Sent handler.
    /// </summary>
    /// <typeparam name="T">The type of event data sent.</typeparam>
    /// <param name="data">The event data sent.</param>
    /// <param name="scope">The scope of the transmitted event.</param>
    public delegate void MyreEventHandler<in T>(T data, object scope);

    /// <summary>
    /// A class which represents an event for a specified data type.
    /// Instances of this type can be used to send events to listeners which have registered with this event.
    /// </summary>
    /// <typeparam name="Data">The type of payload data this event sends.</typeparam>
    public class Event<Data>
    {
        class Invocation
            : IEventInvocation
        {
            static readonly Queue<Invocation> _pool = new Queue<Invocation>();
            private static SpinLock _spinLock = new SpinLock();

            public Data Data;
            public Event<Data> Event;

#if DEBUG
// ReSharper disable NotAccessedField.Local (Debugging Aid)
            public System.Diagnostics.StackTrace Stack;
// ReSharper restore NotAccessedField.Local
#endif

            public void Execute()
            {
                //Loop over event listeners backwards so most recent handlers are executed first
                //This makes events compatible with using them as a chained system where more recent handlers can temporarily block lower handlers (by modifying the event data)
                for (int i = Event._listeners.Count - 1; i >= 0; i--)
                    Event._listeners[i].HandleEvent(Data, Event.Scope);

                Event.TriggerEvent(Data);
            }

            public void Recycle()
            {
                Data = default(Data);
                try
                {
                    _spinLock.Lock();
                    _pool.Enqueue(this);
                }
                finally
                {
                    _spinLock.Unlock();
                }
            }

            public static Invocation Get()
            {
                try
                {
                    _spinLock.Lock();
                    if (_pool.Count > 0)
                        return _pool.Dequeue();
                    else
                        return new Invocation();
                }
                finally
                {
                    _spinLock.Unlock();
                }
            }
        }

        private readonly EventService _service;
        private readonly object _scope;
        private readonly Event<Data> _global;
        private readonly List<IEventListener<Data>> _listeners;

        /// <summary>
        /// Occurs when data is sent along this event instance.
        /// </summary>
        public event MyreEventHandler<Data> Sent;

        /// <summary>
        /// Gets the service.
        /// </summary>
        /// <value>The service.</value>
        public EventService Service { get { return _service; } }

        /// <summary>
        /// Gets the scope object for this event.
        /// </summary>
        public object Scope { get { return _scope; } }

        internal Event(EventService service, object scope = null, Event<Data> globalScoped = null)
        {
            _service = service;
            _scope = scope;
            _global = globalScoped;
            _listeners = new List<IEventListener<Data>>();
        }

        /// <summary>
        /// Adds a listener.
        /// </summary>
        /// <param name="listener">The listener.</param>
        public void AddListener(IEventListener<Data> listener)
        {
            _listeners.Add(listener);
        }

        /// <summary>
        /// Removes a listener.
        /// </summary>
        /// <param name="listener">The listener.</param>
        /// <returns></returns>
        public bool RemoveListener(IEventListener<Data> listener)
        {
            return _listeners.Remove(listener);
        }

        /// <summary>
        /// Sends the specified data to all registered listeners.
        /// </summary>
        /// <param name="data">The data.</param>
        public void Send(Data data)
        {
            Send(data, this);

            if (_global != null)
                Send(data, _global);
        }

        private void Send(Data data, Event<Data> channel)
        {
            Invocation invocation = Invocation.Get();
            invocation.Event = channel;
            invocation.Data = data;
#if DEBUG
            invocation.Stack = new System.Diagnostics.StackTrace();
#endif

            _service.Queue(invocation);
        }

        private void TriggerEvent(Data data)
        {
            if (Sent != null)
                Sent(data, Scope);
        }
    }
}
