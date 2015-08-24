#if !XNA_3_1
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
            this.State = state;
            base.BlockedInputs.Add(0);
        }

        protected override bool Test(TouchDevice device)
        {
            return device.Current.State == State;
        }
    }
}
#endif
