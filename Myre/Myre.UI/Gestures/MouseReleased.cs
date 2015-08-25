using Myre.UI.InputDevices;

namespace Myre.UI.Gestures
{
    public class MouseReleased
        : Gesture<MouseDevice>
    {
        public MouseButtons Button { get; private set; }

        public MouseReleased(MouseButtons button)
            : base(false)
        {
            Button = button;
            BlockedInputs.Add((int)Button);
        }

        protected override bool Test(MouseDevice device)
        {
            return device.IsButtonNewlyUp(Button);
        }
    }
}