using Microsoft.Xna.Framework.Input;
using Myre.UI.InputDevices;

namespace Myre.UI.Gestures
{
    public class ButtonPressed
        : Gesture<GamepadDevice>
    {
        public Buttons Button { get; private set; }

        public ButtonPressed(Buttons button)
            : base(false)
        {
            Button = button;
            BlockedInputs.Add((int)Button);
        }

        protected override bool Test(GamepadDevice device)
        {
            return device.IsButtonNewlyDown(Button);
        }
    }
}