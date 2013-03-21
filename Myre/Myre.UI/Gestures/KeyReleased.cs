using Microsoft.Xna.Framework.Input;
using Myre.UI.InputDevices;

namespace Myre.UI.Gestures
{
    public class KeyReleased
        : Gesture<KeyboardDevice>
    {
        public Keys Key { get; private set; }

        public KeyReleased(Keys key)
            : base(false)
        {
            this.Key = key;
            this.BlockedInputs.Add((int)key);
        }

        protected override bool Test(KeyboardDevice device)
        {
            return device.IsKeyNewlyUp(Key);
        }
    }
}