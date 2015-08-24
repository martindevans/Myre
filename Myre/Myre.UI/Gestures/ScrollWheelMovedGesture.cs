using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Myre.UI.InputDevices;
using Microsoft.Xna.Framework.Input;

namespace Myre.UI.Gestures
{
    class ScrollWhellMovedGesture
        : Gesture<MouseDevice>
    {
        public ScrollWhellMovedGesture()
            : base(false)
        {
            BlockedInputs.Add(1 + 5/*Enum.(typeof(MouseButtons)).Length*/);
        }

        protected override bool Test(MouseDevice device)
        {
            return device.WheelMovement != 0;
        }
    }
}