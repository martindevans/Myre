using System;
using Myre.UI.InputDevices;

namespace Myre.UI.Gestures
{
    public class TriggerMoved
        : Gesture<GamepadDevice>
    {
        public Side Trigger { get; private set; }

        public TriggerMoved(Side trigger)
            : base(false)
        {
            Trigger = trigger;
            BlockedInputs.Add((int)Trigger + 25); //Enum.GetValues(typeof(Buttons)).Length);
        }

        protected override bool Test(GamepadDevice device)
        {
            if (Trigger == Side.Left)
                return Math.Abs(device.LeftTriggerMovement) > float.Epsilon;
            else
                return Math.Abs(device.RightTriggerMovement) > float.Epsilon;
        }
    }
}