using Myre.UI.InputDevices;

namespace Myre.UI.Gestures
{
    public class ScrollWheelMoved
        : Gesture<MouseDevice>
    {
        public ScrollWheelMoved()
            : base(false)
        {
            BlockedInputs.Add(1 + 5/*Enum.(typeof(MouseButtons)).Length*/);
        }

        protected override bool Test(MouseDevice device)
        {
            return device.WheelMovement != 0;
        }
    }
}