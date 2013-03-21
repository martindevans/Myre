using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Myre.UI.Text;

namespace Myre.UI.Controls
{
    /// <summary>
    /// Specifies a growth direction
    /// </summary>
    public enum GrowthDirection
    {
        /// <summary>
        /// Up.
        /// </summary>
        Up,
        /// <summary>
        /// Down.
        /// </summary>
        Down
    }

    /// <summary>
    /// A text log control.
    /// </summary>
    public class TextLog
        : Control
    {
        readonly List<StringPart> _text;
        SpriteFont _font;
        bool _moveNextDrawToNewLine;
        readonly int _historyCapacity;
        int _startIndex;

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
            }
        }

        /// <summary>
        /// Gets or sets the font colour.
        /// </summary>
        public Color Colour
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the justification.
        /// </summary>
        /// <value>The justification.</value>
        public Justification Justification
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the scale.
        /// </summary>
        /// <value>The scale.</value>
        public Vector2 Scale
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the direction text will grow in.
        /// </summary>
        /// <value>The direction.</value>
        public GrowthDirection Direction
        {
            get;
            set;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TextLog"/> class.
        /// </summary>
        /// <param name="parent">This controls parent control.</param>
        /// <param name="font">The font to use to draw the text contained by this log.</param>
        /// <param name="historyCapacity"></param>
        public TextLog(Control parent, SpriteFont font, int historyCapacity)
            : base(parent)
        {
            _historyCapacity = historyCapacity;
            _text = new List<StringPart>();
            Font = font;
            Colour = Color.White;
            Scale = Vector2.One;
            Direction = GrowthDirection.Up;

            //Action<Frame> recalc = delegate(Frame c)
            //{
            //    Recalculate();
            //};

            //AreaChanged += recalc;
        }

        /// <summary>
        /// Writes the line.
        /// </summary>
        /// <param name="line">The line.</param>
        public void WriteLine(StringPart line)
        {
            //var t = text[text.Count - 1];
            //text.RemoveAt(text.Count - 1);
            //t.Batch.Clear();
            //t.Batch.Write(line);
            //t.Height = t.Batch.CalculateArea(Int2D.Zero, Justification, Area.Width).Height;
            //text.Insert(0, t);
            Write(line.ToString());
            _moveNextDrawToNewLine = true;
        }

        /// <summary>
        /// Appends text onto the last line written.
        /// </summary>
        /// <param name="line">The text to append.</param>
        public void Write(StringPart line)
        {
            if (_moveNextDrawToNewLine || _text.Count == 0)
            {
                _text.Add(line);
                _moveNextDrawToNewLine = false;
            }
            else
            {
                var current = _text[_text.Count - 1];
                _text[_text.Count - 1] = current.ToString() + line.ToString();
            }

            //if (line[line.Length - 1] == '\n')
            //    moveNextDrawToNewLine = true;

            _text.RemoveRange(0, Math.Max(0, _text.Count - _historyCapacity));

            if (_startIndex == _text.Count - 1)
                ScrollToNewest();
        }

        /// <summary>
        /// Clears all text from this log
        /// </summary>
        public void Clear()
        {
            //for (int i = 0; i < text.Count; i++)
            //{
            //    text[i].Batch.Clear();
            //}
            _text.Clear();
        }

        /// <summary>
        /// Draws the control and its' children.
        /// </summary>
        /// <param name="batch">An spritebactch already started for alpha blending with deferred sort mode.</param>
        public override void Draw(SpriteBatch batch)
        {
            var heightOffset = 0f;

            for (int i = _startIndex - 1; i >= 0; i--)
            {
                var line = _text[i];
                var size = _font.MeasureParsedString(line, Scale, Area.Width);

                var position = new Vector2(Area.X, Area.Y + (Direction == GrowthDirection.Down ? heightOffset : Area.Height - heightOffset - size.Y));
                batch.DrawParsedString(Font, line, position, Colour, 0, Vector2.Zero, Scale, Area.Width, Justification);

                heightOffset += size.Y;
                if (heightOffset > Area.Height)
                    break;
            }

            base.Draw(batch);
        }

        /// <summary>
        /// Scrolls towards older messages.
        /// </summary>
        public void ScrollBackward()
        {
            _startIndex = Math.Max(0, _startIndex - 1);
        }

        /// <summary>
        /// Scrolls towards newer messages.
        /// </summary>
        public void ScrollForward()
        {
            _startIndex = Math.Min(_text.Count, _startIndex + 1);
        }

        /// <summary>
        /// Scroll to the most recent message.
        /// </summary>
        public void ScrollToNewest()
        {
            _startIndex = _text.Count;
        }
    }
}
