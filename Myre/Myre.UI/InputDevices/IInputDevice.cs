using System.Collections.Generic;

using GameTime = Microsoft.Xna.Framework.GameTime;

namespace Myre.UI.InputDevices
{
    public interface IInputDevice
    {
        InputActor Owner { get; set; }
// ReSharper disable UnusedParameter.Global
        void Update(GameTime gameTime);
// ReSharper restore UnusedParameter.Global
        void Evaluate(GameTime gameTime, Control focused, UserInterface ui);
        bool IsBlocked(IEnumerable<int> inputs);
        void BlockInputs(IEnumerable<int> inputs);
    }
}