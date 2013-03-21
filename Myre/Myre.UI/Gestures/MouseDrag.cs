using Microsoft.Xna.Framework;
using Myre.UI.InputDevices;

namespace Myre.UI.Gestures
{
    public class MouseDrag
        : Gesture<MouseDevice>
    {
        public MouseButtons Button { get; private set; }

        public MouseDrag(MouseButtons button)
            : base(false)
        {
            Button = button;
            BlockedInputs.Add(2 + 5/*Enum.GetValues(typeof(MouseButtons)).Length*/);
            BlockedInputs.Add((int)Button);
        }

        protected override bool Test(MouseDevice device)
        {
            return device.IsButtonDown(Button) && device.PositionMovement != Vector2.Zero;
        }
    }
}