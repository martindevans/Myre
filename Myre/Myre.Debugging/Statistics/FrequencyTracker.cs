using System;

namespace Myre.Debugging.Statistics
{
    /// <summary>
    /// Counts the number of times an events happens in a second.
    /// </summary>
    public class FrequencyTracker
    {
        DateTime _lastUpdate;
        readonly Statistic _statistic;
        int _counter;

        /// <summary>
        /// Gets or sets the frequency.
        /// </summary>
        /// <value>The frequency.</value>
        public float Frequency { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="FrequencyTracker"/> class.
        /// </summary>
        /// <param name="statisticName">Name of the statistic.</param>
        public FrequencyTracker(string statisticName)
        {
            _lastUpdate = DateTime.Now;
            if (!string.IsNullOrEmpty(statisticName))
                _statistic = Statistic.Get(statisticName);
        }

        /// <summary>
        /// Pulses this instance.
        /// </summary>
        public void Pulse()
        {
            _counter++;
            var now = DateTime.Now;
            if ((now - _lastUpdate).TotalSeconds > 1f)
            {
                Frequency = _counter;
                if (_statistic != null)
                    _statistic.Value = _counter;
                _counter = 0;
                _lastUpdate = now;
            }
        }
    }
}
