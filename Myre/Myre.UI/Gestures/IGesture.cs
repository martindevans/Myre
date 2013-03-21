using System;
using System.Collections.Generic;
using Myre.UI.InputDevices;

namespace Myre.UI.Gestures
{
    public interface IGesture
    {
        ICollection<int> BlockedInputs { get; }
        Type DeviceType { get; }
        bool AlwaysEvaluate { get; }
        bool Test(IInputDevice device);
    }

    public abstract class Gesture<Device>
        : IGesture
        where Device : IInputDevice
    {
        private static readonly Type _deviceType = typeof(Device);
        private readonly List<int> _blockedInputs = new List<int>();

        public ICollection<int> BlockedInputs
        {
            get { return _blockedInputs; }
        }

        public Type DeviceType
        {
            get { return _deviceType; }
        }

        public bool AlwaysEvaluate
        {
            get;
            private set;
        }

        protected Gesture(bool alwaysEvaluate)
        {
            AlwaysEvaluate = alwaysEvaluate;
        }

        protected abstract bool Test(Device device);

        bool IGesture.Test(IInputDevice device)
        {
            return Test((Device)device);
        }
    }
}