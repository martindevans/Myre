using Microsoft.Xna.Framework.Input.Touch;
using Myre.UI.InputDevices;

namespace Myre.UI.Gestures
{
    public class TouchChangedState
        : Gesture<TouchDevice>
    {
        public TouchLocationState State { get; set; }

        public TouchChangedState(TouchLocationState state)
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