using Myre.UI.InputDevices;
using Microsoft.Xna.Framework.Input.Touch;

namespace Myre.UI.Gestures
{
    public class TouchGesture
        : Gesture<TouchDevice>
    {
        public TouchLocationState State { get; set; }

        public TouchGesture(TouchLocationState state)
            : base(false)
        {
            State = state;
            BlockedInputs.Add(0);
        }

        protected override bool Test(TouchDevice device)
        {
            return device.Current.State == State;
        }
    }
}
