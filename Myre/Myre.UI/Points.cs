using System;

namespace Myre.UI
{
    /// <summary>
    /// An enum specifying parts of a Frame.
    /// </summary>
    [Flags]
    public enum Points
    {
        /// <summary>
        /// The centre of the left edge.
        /// </summary>
        Left = 1 << 0,
        /// <summary>
        /// The centre of the right edge.
        /// </summary>
        Right = 1 << 1,
        /// <summary>
        /// The centre of the top edge.
        /// </summary>
        Top = 1 << 2,
        /// <summary>
        /// The centre of the bottom edge.
        /// </summary>
        Bottom = 1 << 3,
        /// <summary>
        /// The centre.
        /// </summary>
        Centre = 1 << 4,
        /// <summary>
        /// The top left corner.
        /// </summary>
        TopLeft = Top | Left,
        /// <summary>
        /// The top right corner.
        /// </summary>
        TopRight = Top | Right,
        /// <summary>
        /// The bottom left corner.
        /// </summary>
        BottomLeft = Bottom | Left,
        /// <summary>
        /// The bottom right corner.
        /// </summary>
        BottomRight = Bottom | Right
    }

    static class PointsExtensions
    {
        public static bool Selected(this Points points, Points option)
        {
            return (points & option) == option;
        }
    }
}
