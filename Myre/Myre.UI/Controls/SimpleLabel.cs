using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Myre.UI.Controls
{
    /// <summary>
    /// A simple label control for drawing text. Does not support glyphs or text wrapping.
    /// Use this for labels which change often.
    /// </summary>
    public class SimpleLabel
        : Control
    {
        private string _text;
        private SpriteFont _font;

        /// <summary>
        /// Gets or sets the text.
        /// </summary>
        public string Text 
        {
            get { return _text; }
            set
            {
                if (value == null)
                    throw new ArgumentNullException("value");
                if (!_text.Equals(value))
                {
                    _text = value;
                    SetSize((Int2D)_font.MeasureString(_text));
                }
            }
        }

        /// <summary>
        /// Gets or sets the font.
        /// </summary>
        public SpriteFont Font
        {
            get { return _font; }
            set
            {
                if (value == null)
                    throw new ArgumentNullException("value");
                _font = value;
                SetSize(new Int2D(Area.Width, Font.LineSpacing));
            }
        }

        /// <summary>
        /// Gets or sets the colour.
        /// </summary>
        public Color Colour { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SimpleLabel"/> class.
        /// </summary>
        /// <param name="parent">This controls parent control.</param>
        /// <param name="font">The font.</param>
        public SimpleLabel(Control parent, SpriteFont font)
            : base(parent)
        {
            if (font == null)
                throw new ArgumentNullException("font");

            _text = "";
            Font = font;
            SetSize((Int2D)font.MeasureString(_text));

            Action<Frame> recalculateSize = c => SetSize(new Int2D(Area.Width, Font.LineSpacing));

            AreaChanged += recalculateSize;
        }

        /// <summary>
        /// Draws the control.
        /// </summary>
        /// <param name="batch">An spritebactch already started for alpha blending with deferred sort mode.</param>
        public override void Draw(SpriteBatch batch)
        {
            batch.DrawString(Font, Text, new Vector2(Area.X, Area.Y), Colour);
        }
    }
}
