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
                return device.LeftTriggerMovement != 0;
            else
                return device.RightTriggerMovement != 0;
        }
    }
}