using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

namespace Myre.Debugging.Statistics
{
    /// <summary>
    /// A statistic which can be tracked by tools in the Myre.Debugging library.
    /// </summary>
    public sealed class Statistic
    {
        static readonly ConcurrentDictionary<string, Statistic> _statistics = new ConcurrentDictionary<string, Statistic>();

        public class StatisticsCollection
            : IEnumerable<KeyValuePair<string, Statistic>>
        {
            public Statistic this[string key]
            {
                get
                {
                    Statistic value;
                    _statistics.TryGetValue(key, out value);
                    return value;
                }
            }

            public IEnumerator<KeyValuePair<string, Statistic>> GetEnumerator()
            {
                return _statistics.GetEnumerator();
            }

            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }

        static readonly StatisticsCollection _statsCollection = new StatisticsCollection();
// ReSharper disable once ReturnTypeCanBeEnumerable.Global
        public static StatisticsCollection Statistics
        {
            get
            {
                return _statsCollection;
            }
        }
        

        /// <summary>
        /// Gets the name.
        /// </summary>
        /// <value>The name.</value>
        public string Name { get; private set; }

        private SpinLock _valueLock = new SpinLock();
        private float _value;

        /// <summary>
        /// Gets the value.
        /// </summary>
        /// <value>The value.</value>
        public float Value
        {
            get
            {
                return _value;
            }
        }

        public void Add(float value)
        {
            bool taken = false;
            _valueLock.Enter(ref taken);
            try
            {
                _value += value;
            }
            finally
            {
                if (taken)
                    _valueLock.Exit();
            }
        }

        public void Set(float value)
        {
            bool taken = false;
            _valueLock.Enter(ref taken);
            try
            {
                _value = value;
            }
            finally
            {
                if (taken)
                    _valueLock.Exit();
            }
        }

        /// <summary>
        /// Gets a value indicating whether this instance is disposed.
        /// </summary>
        /// <value></value>
        public bool IsDisposed { get; private set; }

        /// <summary>
        /// Gets or sets the format.
        /// </summary>
        /// <value>The format.</value>
        public string Format { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Statistic"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        private Statistic(string name)
        {
            Name = name;
        }

        /// <summary>
        /// Gets the statistic.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="format"></param>
        /// <returns></returns>
        public static Statistic Create(string name, string format = null)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentNullException("name");

            var stat = _statistics.GetOrAdd(name, a => new Statistic(name));
            stat.Format = format ?? stat.Format ?? "{0}";
            return stat;
        }
    }
}
