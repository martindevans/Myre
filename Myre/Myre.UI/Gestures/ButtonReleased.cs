using Microsoft.Xna.Framework.Input;
using Myre.UI.InputDevices;

namespace Myre.UI.Gestures
{
    public class ButtonReleased
        : Gesture<GamepadDevice>
    {
        public Buttons Button { get; private set; }

        public ButtonReleased(Buttons button)
            : base(false)
        {
            Button = button;
            BlockedInputs.Add((int)Button);
        }

        protected override bool Test(GamepadDevice device)
        {
            return device.IsButtonNewlyUp(Button);
        }
    }
}