using System;

namespace Myre.Debugging.Statistics
{
    public class StatisticTracker
    {
        readonly Statistic _stat;
        DateTime _lastAccess;
        readonly TimeSpan _accessInterval;
        float _lastValue;

        public Statistic Statistic
        {
            get { return _stat; }
        }

        public StatisticTracker(Statistic statistic, TimeSpan accessInterval)
        {
            _stat = statistic;
            _accessInterval = accessInterval;
            _lastAccess = DateTime.Now;
            _lastValue = statistic.Value;
        }

        public float GetValue(out bool read, out bool changed)
        {
            if (_stat.IsDisposed)
            {
                read = false;
                changed = false;
                return _lastValue;
            }

            changed = false;
            read = false;
            var now = DateTime.Now;
            var dt = now - _lastAccess;
            if (dt >= _accessInterval)
            {
// ReSharper disable CompareOfFloatsByEqualityOperator
                changed = _lastValue != _stat.Value;
// ReSharper restore CompareOfFloatsByEqualityOperator
                _lastValue = _stat.Value;
                _lastAccess += _accessInterval;
                read = true;
            }

            return _lastValue;
        }
    }
}
