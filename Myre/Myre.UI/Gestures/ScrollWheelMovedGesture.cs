using System;
using Myre.UI.InputDevices;

namespace Myre.UI.Gestures
{
    class ScrollWhellMovedGesture
        : Gesture<MouseDevice>
    {
        public ScrollWhellMovedGesture()
            : base(false)
        {
            BlockedInputs.Add(1 + 5/*Enum.(typeof(MouseButtons)).Length*/);
        }

        protected override bool Test(MouseDevice device)
        {
            return Math.Abs(device.WheelMovement) > float.Epsilon;
        }
    }
}