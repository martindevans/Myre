using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Myre.UI.InputDevices;

namespace Myre.UI.Gestures
{
    public class MouseReleasedGesture
        : Gesture<MouseDevice>
    {
        public MouseButtons Button { get; private set; }

        public MouseReleasedGesture(MouseButtons button)
            : base(false)
        {
            Button = button;
            base.BlockedInputs.Add((int)Button);
        }

        protected override bool Test(MouseDevice device)
        {
            return device.IsButtonNewlyUp(Button);
        }
    }
}