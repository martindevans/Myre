using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Myre.Extensions;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace Myre.Entities.Services
{
    class ServiceContainer
        : IEnumerable<IService>
    {
        private Dictionary<Type, IService> dictionary;
        private List<IService> update;
        private List<IService> draw;        

        private List<IService> buffer;
        private bool dirty;

        private Comparison<IService> updateOrder;
        private Comparison<IService> drawOrder;

        Stopwatch timer = new Stopwatch();
        private List<KeyValuePair<IService, TimeSpan>> executionTimes;
        public ReadOnlyCollection<KeyValuePair<IService, TimeSpan>> ExecutionTimes;

        public IService this[Type type]
        {
            get { return dictionary[type]; }
        }

        public ServiceContainer()
        {
            dictionary = new Dictionary<Type, IService>();
            update = new List<IService>();
            draw = new List<IService>();
            buffer = new List<IService>();

            executionTimes = new List<KeyValuePair<IService, TimeSpan>>();
            ExecutionTimes = new ReadOnlyCollection<KeyValuePair<IService, TimeSpan>>(executionTimes);

            updateOrder = (a, b) => a.UpdateOrder.CompareTo(b.UpdateOrder);
            drawOrder = (a, b) => a.DrawOrder.CompareTo(b.DrawOrder);
        }

        public void Add(IService service)
        {
            buffer.Add(service);
            dictionary[service.GetType()] = service;
            dirty = true;
        }

        public bool Remove(IService service)
        {
            var removed = buffer.Remove(service);

            if (removed)
            {
                dictionary.Remove(service.GetType());
                dirty = true;
            }

            return removed;
        }

        public bool TryGet(Type serviceType, out IService service)
        {
            if (dictionary.TryGetValue(serviceType, out service))
                return true;

            foreach (var item in buffer)
            {
                if (serviceType.IsAssignableFrom(item.GetType()))
                {
                    service = item;
                    return true;
                }
            }

            return false;
        }

        public void Clear()
        {
            buffer.Clear();
            dirty = true;
        }

        public void Update(float elapsedTime)
        {
            UpdateLists();
            update.InsertionSort(updateOrder);

            executionTimes.Clear();

            for (int i = 0; i < update.Count; i++)
            {
                timer.Restart();
                update[i].Update(elapsedTime);
                timer.Stop();
                executionTimes.Add(new KeyValuePair<IService, TimeSpan>(update[i], timer.Elapsed));
            }
        }

        public void Draw()
        {
            UpdateLists();
            draw.InsertionSort(drawOrder);
            for (int i = 0; i < draw.Count; i++)
                draw[i].Draw();
        }

        private void UpdateLists()
        {
            RemoveDisposed();

            if (!dirty)
                return;

            update.Clear();
            update.AddRange(buffer);

            draw.Clear();
            draw.AddRange(buffer);

            dirty = false;
        }

        private void RemoveDisposed()
        {
            for (int i = buffer.Count - 1; i >= 0; i--)
            {
                if (buffer[i].IsDisposed)
                {
                    buffer.RemoveAt(i);
                    dirty = true;
                }
            }
        }

        public IEnumerator<IService> GetEnumerator()
        {
            return buffer.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
