using Myre.UI.InputDevices;

namespace Myre.UI.Gestures
{
    public class CharactersEntered
        : Gesture<KeyboardDevice>
    {
        public CharactersEntered()
            : base(false)
        {
            this.BlockedInputs.Add(-1);
        }

        protected override bool Test(KeyboardDevice device)
        {
            return device.Characters.Count > 0;
        }
    }
}