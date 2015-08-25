using Microsoft.Xna.Framework.Input;
using Myre.UI.InputDevices;

namespace Myre.UI.Gestures
{
    public class KeyReleasedGesture
        : Gesture<KeyboardDevice>
    {
        public Keys Key { get; private set; }

        public KeyReleasedGesture(Keys key)
            : base(false)
        {
            Key = key;
            BlockedInputs.Add((int)key);
        }

        protected override bool Test(KeyboardDevice device)
        {
            return device.IsKeyNewlyUp(Key);
        }
    }
}