using Microsoft.Xna.Framework.Input;

namespace Myre.UI.InputDevices
{
    /// <summary>
    /// An enum representing the buttons on a mouse.
    /// </summary>
    public enum MouseButtons
    {
        /// <summary>
        /// The left button.
        /// </summary>
        Left,

        /// <summary>
        /// The middle button.
        /// </summary>
        Middle,

        /// <summary>
        /// The right button.
        /// </summary>
        Right,

        /// <summary>
        /// The first extra button.
        /// </summary>
        X1,

        /// <summary>
        /// The second extra button.
        /// </summary>
        X2
    }

    /// <summary>
    /// A static class containing extension methods for the MouseState struct.
    /// </summary>
    public static class MouseStateExtensions
    {
        /// <summary>
        /// Determines if the specifies mouse button is pressed.
        /// </summary>
        /// <param name="state"></param>
        /// <param name="button"></param>
        /// <returns><c>true</c> if the button is pressed; else <c>false</c>.</returns>
        public static bool IsButtonDown(this MouseState state, MouseButtons button)
        {
            switch (button)
            {
                case MouseButtons.Left:
                    return state.LeftButton == ButtonState.Pressed;
                case MouseButtons.Middle:
                    return state.MiddleButton == ButtonState.Pressed;
                case MouseButtons.Right:
                    return state.RightButton == ButtonState.Pressed;
                case MouseButtons.X1:
                    return state.XButton1 == ButtonState.Pressed;
                case MouseButtons.X2:
                    return state.XButton2 == ButtonState.Pressed;
                default:
                    return false;
            }
        }

        /// <summary>
        /// Determines if the specifies mouse button is released.
        /// </summary>
        /// <param name="state"></param>
        /// <param name="button"></param>
        /// <returns><c>true</c> if the button is released; else <c>false</c>.</returns>
        public static bool IsButtonUp(this MouseState state, MouseButtons button)
        {
            return !state.IsButtonDown(button);
        }
    }
}
