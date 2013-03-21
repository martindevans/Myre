using Microsoft.Xna.Framework;
using Myre.UI.InputDevices;

namespace Myre.UI.Gestures
{
    public class MouseMove
        : Gesture<MouseDevice>
    {
        public MouseMove()
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