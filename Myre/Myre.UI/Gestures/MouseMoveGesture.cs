using System.Numerics;
using Myre.UI.InputDevices;

namespace Myre.UI.Gestures
{
    class MouseMoveGesture
        : Gesture<MouseDevice>
    {
        public MouseMoveGesture()
            : base(false)
        {
            BlockedInputs.Add(2 + 5/*Enum.GetValues(typeof(MouseButtons)).Length*/);
        }

        protected override bool Test(MouseDevice device)
        {
            return device.PositionMovement != Vector2.Zero;
        }
    }
}