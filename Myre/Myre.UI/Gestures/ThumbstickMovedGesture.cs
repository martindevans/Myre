using System.Numerics;
using Myre.UI.InputDevices;

namespace Myre.UI.Gestures
{
    class ThumbstickMovedGesture
        : Gesture<GamepadDevice>
    {
        public Side Thumbstick { get; private set; }

        public ThumbstickMovedGesture(Side thumbstick)
            : base(false)
        {
            Thumbstick = thumbstick;
            BlockedInputs.Add((int)Thumbstick + 25);//Enum.GetValues(typeof(Buttons)).Length);
        }

        protected override bool Test(GamepadDevice device)
        {
            if (Thumbstick == Side.Left)
                return device.LeftThumbstickMovement != Vector2.Zero;
            else
                return device.RightThumbstickMovement != Vector2.Zero;
        }
    }
}