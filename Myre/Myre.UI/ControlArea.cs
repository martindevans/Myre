
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace Myre.UI
{
    struct ControlArea
    {
        public int Top, Bottom, Left, Right;

        public ControlArea(Rectangle area)
            : this(area.X, area.Y, area.Width, area.Height)
        {
        }

        public ControlArea(int x, int y, int width, int height)
            : this()
        {
            Top = y;
            Bottom = y + height;
            Left = x;
            Right = x + width;
        }

        public Rectangle ToRectangle()
        {
            return new Rectangle(Left, Top, Right - Left, Bottom - Top);
        }
    }
}
