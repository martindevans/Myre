using Myre.UI.InputDevices;
using System;

namespace Myre.UI.Gestures
{
    class TriggerMovedGesture
        : Gesture<GamepadDevice>
    {
        public Side Trigger { get; private set; }

        public TriggerMovedGesture(Side trigger)
            : base(false)
        {
            Trigger = trigger;
            BlockedInputs.Add((int)Trigger + 25); //Enum.GetValues(typeof(Buttons)).Length);
        }

        protected override bool Test(GamepadDevice device)
        {
            const float EPSILON = 0.01f;
            if (Trigger == Side.Left)
                return Math.Abs(device.LeftTriggerMovement) > EPSILON;
            else
                return Math.Abs(device.RightTriggerMovement) > EPSILON;
        }
    }
}