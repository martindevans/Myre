using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using Myre.Extensions;

namespace Myre.Entities.Services
{
    class ServiceContainer
        : IEnumerable<IService>
    {
        private readonly Dictionary<Type, IService> _dictionary = new Dictionary<Type, IService>();
        private readonly List<IService> _update = new List<IService>();
        private readonly List<IService> _draw = new List<IService>();

        private readonly List<IService> _buffer = new List<IService>();
        private bool _dirty;

        private readonly Comparison<IService> _updateOrder;
        private readonly Comparison<IService> _drawOrder;

        private readonly Stopwatch _timer = new Stopwatch();
        private readonly List<KeyValuePair<IService, TimeSpan>> _executionTimes = new List<KeyValuePair<IService, TimeSpan>>();
        public IReadOnlyList<KeyValuePair<IService, TimeSpan>> ExecutionTimes
        {
            get { return _executionTimes; }
        }

        private readonly List<KeyValuePair<IService, TimeSpan>> _renderTimes = new List<KeyValuePair<IService, TimeSpan>>();
        public IReadOnlyList<KeyValuePair<IService, TimeSpan>> RenderTimes
        {
            get { return _renderTimes; }
        }

        public IService this[Type type]
        {
            get
            {
                Contract.Requires(type != null);
                Contract.Ensures(Contract.Result<IService>() != null);

                var item = _dictionary[type];
                Contract.Assume(item != null);
                return item;
            }
        }

        public ServiceContainer()
        {
            _updateOrder = (a, b) => a.UpdateOrder.CompareTo(b.UpdateOrder);
            _drawOrder = (a, b) => a.DrawOrder.CompareTo(b.DrawOrder);
        }

        public void Add(IService service)
        {
            Contract.Requires(service != null);

            _buffer.Add(service);
            _dictionary[service.GetType()] = service;
            _dirty = true;
        }

        public bool Remove(IService service)
        {
            var removed = _buffer.Remove(service);

            if (removed)
            {
                _dictionary.Remove(service.GetType());
                _dirty = true;
            }

            return removed;
        }

        public bool TryGet(Type serviceType, out IService service)
        {
            Contract.Requires(serviceType != null);

            if (_dictionary.TryGetValue(serviceType, out service))
                return true;

            foreach (var item in _buffer)
            {
                if (serviceType.IsInstanceOfType(item))
                {
                    service = item;
                    return true;
                }
            }

            return false;
        }

        public void Clear()
        {
            _buffer.Clear();
            _dirty = true;
        }

        public void Update(float elapsedTime)
        {
            UpdateLists();
            _update.InsertionSort(_updateOrder);

            _executionTimes.Clear();

            foreach (var service in _update)
            {
                _timer.Restart();
                service.Update(elapsedTime);
                _timer.Stop();
                _executionTimes.Add(new KeyValuePair<IService, TimeSpan>(service, _timer.Elapsed));
            }
        }

        public void Draw()
        {
            UpdateLists();
            _draw.InsertionSort(_drawOrder);

            _renderTimes.Clear();

            foreach (var service in _draw)
            {
                _timer.Restart();
                service.Draw();
                _timer.Stop();
                _renderTimes.Add(new KeyValuePair<IService, TimeSpan>(service, _timer.Elapsed));
            }
        }

        private void UpdateLists()
        {
            RemoveDisposed();

            if (!_dirty)
                return;

            _update.Clear();
            _update.AddRange(_buffer);

            _draw.Clear();
            _draw.AddRange(_buffer);

            _dirty = false;
        }

        private void RemoveDisposed()
        {
            for (var i = _buffer.Count - 1; i >= 0; i--)
            {
                var item = _buffer[i];
                Contract.Assume(item != null);
                if (item.IsDisposed)
                {
                    _buffer.RemoveAt(i);
                    _dirty = true;
                }
            }
        }

        public IEnumerator<IService> GetEnumerator()
        {
            return _buffer.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
