using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Myre.Entities.Services
{
    /// <summary>
    /// An interface which defines a service to manage processes.
    /// </summary>
    public interface IProcessService
        : IService
    {
        /// <summary>
        /// Adds the specified process to this instance.
        /// </summary>
        /// <param name="process">The process to add.</param>
        void Add(IProcess process);
    }

    /// <summary>
    /// A class which manages the updating of processes.
    /// </summary>
    public class ProcessService
        : Service, IProcessService
    {
        private readonly List<IProcess> _processes;
        private readonly List<IProcess> _buffer;

        readonly Stopwatch _timer = new Stopwatch();
        private readonly List<KeyValuePair<IProcess, TimeSpan>> _executionTimes;
        private readonly ReadOnlyCollection<KeyValuePair<IProcess, TimeSpan>> _readonlyExecutionTimes;

        /// <summary>
        /// A collection of diagnostic data about service execution time
        /// </summary>
        public ReadOnlyCollection<KeyValuePair<IProcess, TimeSpan>> ProcessExecutionTimes
        {
            get { return _readonlyExecutionTimes; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ProcessService"/> class.
        /// </summary>
        public ProcessService()
        {
            _processes = new List<IProcess>();
            _buffer = new List<IProcess>();

            _executionTimes = new List<KeyValuePair<IProcess, TimeSpan>>();
            _readonlyExecutionTimes = new ReadOnlyCollection<KeyValuePair<IProcess, TimeSpan>>(_executionTimes);
        }

        /// <summary>
        /// Updates the all non-complete processes.
        /// </summary>
        /// <param name="elapsedTime">The number of seconds which have elapsed since the previous frame.</param>
        public override void Update(float elapsedTime)
        {
            lock (_buffer)
            {
                _processes.AddRange(_buffer);
                _buffer.Clear();
            }

            _executionTimes.Clear();

            for (int i = _processes.Count - 1; i >= 0; i--)
            {
                var process = _processes[i];

                if (process.IsComplete)
                {
                    _processes.RemoveAt(i);
                    continue;
                }

                _timer.Restart();

                process.Update(elapsedTime);

                _timer.Stop();
                _executionTimes.Add(new KeyValuePair<IProcess, TimeSpan>(process, _timer.Elapsed));
            }
        }

        /// <summary>
        /// Adds the specified process.
        /// </summary>
        /// <param name="process">The process.</param>
        public void Add(IProcess process)
        {
            lock (_buffer)
                _buffer.Add(process);
        }

        public void Add(Func<float, bool> update)
        {
            Add(new ActionProcess(update));
        }

        private class ActionProcess : IProcess
        {
            private readonly Func<float, bool> _update;

            public ActionProcess(Func<float, bool> update)
            {
                _update = update;
            }

            public bool IsComplete { get; private set; }

            public void Update(float elapsedTime)
            {
                if (!IsComplete)
                    IsComplete = _update(elapsedTime);
            }
        }
    }
}
