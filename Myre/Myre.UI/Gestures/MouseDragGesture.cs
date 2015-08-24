using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Myre.UI.InputDevices;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;

namespace Myre.UI.Gestures
{
    public class MouseDragGesture
        : Gesture<MouseDevice>
    {
        public MouseButtons Button { get; private set; }

        public MouseDragGesture(MouseButtons button)
            : base(false)
        {
            Button = button;
            BlockedInputs.Add(2 + 5/*Enum.GetValues(typeof(MouseButtons)).Length*/);
            BlockedInputs.Add((int)Button);
        }

        protected override bool Test(MouseDevice device)
        {
            return device.IsButtonDown(Button) && device.PositionMovement != Vector2.Zero;
        }
    }
}