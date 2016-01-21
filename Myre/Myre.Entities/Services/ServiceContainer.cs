using System;
using System.Collections.Generic;
using System.Diagnostics;
using Myre.Extensions;

namespace Myre.Entities.Services
{
    class ServiceContainer
        : IEnumerable<IService>
    {
        private readonly Dictionary<Type, IService> _dictionary;
        private readonly List<IService> _update;
        private readonly List<IService> _draw;        

        private readonly List<IService> _buffer;
        private bool _dirty;

        private readonly Comparison<IService> _updateOrder;
        private readonly Comparison<IService> _drawOrder;

        readonly Stopwatch _timer = new Stopwatch();
        private readonly List<KeyValuePair<IService, TimeSpan>> _executionTimes;
        public IReadOnlyList<KeyValuePair<IService, TimeSpan>> ExecutionTimes
        {
            get { return _executionTimes; }
        }

        private readonly List<KeyValuePair<IService, TimeSpan>> _renderTimes;
        public IReadOnlyList<KeyValuePair<IService, TimeSpan>> RenderTimes
        {
            get { return _renderTimes; }
        }

        public IService this[Type type]
        {
            get { return _dictionary[type]; }
        }

        public ServiceContainer()
        {
            _dictionary = new Dictionary<Type, IService>();
            _update = new List<IService>();
            _draw = new List<IService>();
            _buffer = new List<IService>();

            _executionTimes = new List<KeyValuePair<IService, TimeSpan>>();
            _renderTimes = new List<KeyValuePair<IService, TimeSpan>>();

            _updateOrder = (a, b) => a.UpdateOrder.CompareTo(b.UpdateOrder);
            _drawOrder = (a, b) => a.DrawOrder.CompareTo(b.DrawOrder);
        }

        public void Add(IService service)
        {
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
            for (int i = _buffer.Count - 1; i >= 0; i--)
            {
                if (_buffer[i].IsDisposed)
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
