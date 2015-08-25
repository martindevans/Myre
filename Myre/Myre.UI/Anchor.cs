using System.Numerics;

using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace Myre.UI
{
    class Anchor
    {
        public Points Start;
        public Points End;
        public Frame AnchorControl;
        public Vector2 Offset;

        public void Apply(ref ControlArea area, Rectangle parentArea, Points anchoredPoints)
        {
            if (AnchorControl != null)
                parentArea = AnchorControl.Area;

            Vector2 pointOnSelf = FindPosition(area.ToRectangle(), Start);
            Vector2 pointOnParent = FindPosition(parentArea, End);

            TranslateArea(ref area, pointOnParent - pointOnSelf + Offset, anchoredPoints);
        }

        private static Vector2 FindPosition(Rectangle area, Points point)
        {
            switch (point)
            {
                case Points.Left:
                    return new Vector2(area.Left, area.Top + (area.Height / 2));
                case Points.Right:
                    return new Vector2(area.Right, area.Top + (area.Height / 2));
                case Points.Top:
                    return new Vector2(area.Left + (area.Width / 2), area.Top);
                case Points.Bottom:
                    return new Vector2(area.Left + (area.Width / 2), area.Bottom);
                case Points.Centre:
                    return new Vector2(area.Center.X, area.Center.Y);
                case Points.TopLeft:
                    return new Vector2(area.X, area.Y);
                case Points.TopRight:
                    return new Vector2(area.X + area.Width, area.Y);
                case Points.BottomLeft:
                    return new Vector2(area.X, area.Y + area.Height);
                case Points.BottomRight:
                    return new Vector2(area.X + area.Width, area.Y + area.Height);
                default:
                    return Vector2.Zero;
            }
        }

        private void TranslateArea(ref ControlArea area, Vector2 translation, Points anchoredPoints)
        {
            // an anchor can move a side if it has direct control of that side
            // or it can move the opposite side is that one has no direct anchors
            // or it can move both of the other 2 sides if neither of those have direct anchors.

            var leftOrRightAnchored = EitherSelected(anchoredPoints, Points.Left, Points.Right);
            var topOrBottomAnchored = EitherSelected(anchoredPoints, Points.Top, Points.Bottom);

            // left and right
            if (Start.Selected(Points.Left) || (Start.Selected(Points.Right) && !anchoredPoints.Selected(Points.Left)) || !leftOrRightAnchored)
                area.Left += (int)translation.X;
            if (Start.Selected(Points.Right) || (Start.Selected(Points.Left) && !anchoredPoints.Selected(Points.Right)) || !leftOrRightAnchored)
                area.Right += (int)translation.X;

            // top and bottom
            if (Start.Selected(Points.Top) || (Start.Selected(Points.Bottom) && !anchoredPoints.Selected(Points.Top)) || !topOrBottomAnchored)
                area.Top += (int)translation.Y;
            if (Start.Selected(Points.Bottom) || (Start.Selected(Points.Top) && !anchoredPoints.Selected(Points.Bottom)) || !topOrBottomAnchored)
                area.Bottom += (int)translation.Y;
        }

        private bool EitherSelected(Points points, Points a, Points b)
        {
            var masked = points & (a | b);
            return (int)masked > 0;
        }
    }
}
