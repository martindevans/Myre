
namespace Myre.Extensions
{
    /// <summary>
    /// A static class containing extension methods for the Microsoft.Xna.Framework.GameTime class.
    /// </summary>
    public static class GameTimeExtensions
    {
        /// <summary>
        /// Gets the number of seconds elapsed since the last frame.
        /// </summary>
        /// <param name="t">The t.</param>
        /// <returns>The number of seconds elapsed since the last frame.</returns>
        public static float Seconds(this Microsoft.Xna.Framework.GameTime t)
        {
            return (float)t.ElapsedGameTime.TotalSeconds;
        }

        /// <summary>
        /// Gets the total number of seconds elapsed since the game started.
        /// </summary>
        /// <param name="t">The t.</param>
        /// <returns>The total number of seconds elapsed since the game started.</returns>
        public static double TotalSeconds(this Microsoft.Xna.Framework.GameTime t)
        {
            return (float)t.TotalGameTime.TotalSeconds;
        }
    }
}
