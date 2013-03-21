using Myre.UI.InputDevices;

namespace Myre.UI.Gestures
{
    public class MousePressed
        : Gesture<MouseDevice>
    {
        public MouseButtons Button { get; private set; }

        public MousePressed(MouseButtons button)
            : base(false)
        {
            Button = button;
            base.BlockedInputs.Add((int)Button);
        }

        protected override bool Test(MouseDevice device)
        {
            return device.IsButtonNewlyDown(Button);
        }
    }
}
