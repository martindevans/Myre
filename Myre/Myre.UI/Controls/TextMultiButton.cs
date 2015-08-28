using System;
using System.Numerics;
using Microsoft.Xna.Framework.Graphics;
using Myre.UI.Gestures;
using Myre.UI.InputDevices;
using Myre.UI.Text;

using GameTime = Microsoft.Xna.Framework.GameTime;
using Color = Microsoft.Xna.Framework.Color;

namespace Myre.UI.Controls
{
    /// <summary>
    /// A text multibutton.
    /// </summary>
    public class TextMultiButton
        : MultiButton
    {
        private readonly string[] _options;
        private readonly Label _leftArrow;
        private readonly Label _rightArrow;
        private readonly Label _centreText;

        /// <summary>
        /// Gets or sets the text.
        /// </summary>
        /// <value>The text.</value>
        public StringPart Text
        {
            get { return _centreText.Text; }
            set { _centreText.Text = value; }
        }

        /// <summary>
        /// Gets or sets the justification.
        /// </summary>
        public override Justification Justification
        {
            get { return _centreText.Justification; }
            set { _centreText.Justification = value; }
        }

        /// <summary>
        /// Gets the option at the specified index.
        /// </summary>
        /// <value></value>
        public string this[int i]
        {
            get { return _options[i]; }
        }

        /// <summary>
        /// Gets or sets the font colour.
        /// </summary>
        public Color Colour { get; set; }

        /// <summary>
        /// Gets or sets the highlight font colour.
        /// </summary>
        public Color Highlight { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="TextMultiButton"/> class.
        /// </summary>
        /// <param name="parent">This controls parent control.</param>
        /// <param name="font">The font.</param>
        /// <param name="options">The options.</param>
        public TextMultiButton(Control parent, SpriteFont font, string[] options)
            : base(parent)
        {
            if (options == null)
                throw new ArgumentNullException("options");
            if (options.Length == 0)
                throw new ArgumentException("There must be at least one option.", "options");

            Colour = Color.White;
            Highlight = Color.CornflowerBlue;
            _options = options;
            OptionsCount = options.Length;

            _leftArrow = new Label(this, font) {Text = "<"};
            _leftArrow.SetPoint(Points.TopLeft, 0, 0);
            _rightArrow = new Label(this, font) {Text = ">"};
            _rightArrow.SetPoint(Points.TopRight, 0, 0);

            _centreText = new Label(this, font) {Justification = Justification.Centre};
            _centreText.SetPoint(Points.TopLeft, _leftArrow.Area.Width, 0);
            _centreText.SetPoint(Points.TopRight, -_rightArrow.Area.Width, 0);
            _centreText.Text = options[0];

            ControlEventHandler recalcSize = delegate
            {
                Vector2 maxSize = Vector2.Zero;
                for (int i = 0; i < options.Length; i++)
                {
                    var size = font.MeasureString(options[i]);
                    maxSize.X = Math.Max(maxSize.X, size.X);
                    maxSize.Y = Math.Max(maxSize.Y, size.Y);
                }
                int arrowSize = (int)font.MeasureString("<").X;
                maxSize.X += arrowSize * 2;
                SetSize((int)maxSize.X, (int)maxSize.Y);
                _leftArrow.SetSize(arrowSize, font.LineSpacing);
                _rightArrow.SetSize(arrowSize, font.LineSpacing);
            };

            ControlEventHandler highlight = delegate(Control c)
            {
                ((Label)c).Colour = (c.IsFocused || c.IsWarm) ? Highlight : Colour;
            };

            _leftArrow.WarmChanged += highlight;
            _rightArrow.WarmChanged += highlight;
            recalcSize(this);

            SelectionChanged += delegate {
                _centreText.Text = this[SelectedOption];
            };

            BindGestures();
        }

        /// <summary>
        /// Binds the left and right buttons to next and previous.
        /// </summary>
        private void BindGestures()
        {
            _leftArrow.Gestures.Bind((GestureHandler<IGesture>)PreviousOption,
                new MouseReleased(MouseButtons.Left));

            _rightArrow.Gestures.Bind((GestureHandler<IGesture>)NextOption,
                new MouseReleased(MouseButtons.Left));
        }

        private void NextOption(IGesture gesture, GameTime time, IInputDevice device)
        {
            SelectedOption++;
        }

        private void PreviousOption(IGesture gesture, GameTime time, IInputDevice device)
        {
            SelectedOption--;
        }

        /// <summary>
        /// Draws the control and its' children.
        /// </summary>
        /// <param name="batch">An spritebactch already started for alpha blending with deferred sort mode.</param>
        public override void Draw(SpriteBatch batch)
        {
            _centreText.Colour = IsFocused ? Highlight : Colour;
        }
    }
}
