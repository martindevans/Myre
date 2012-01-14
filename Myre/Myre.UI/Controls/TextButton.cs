using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Myre.UI.Text;

namespace Myre.UI.Controls
{
    /// <summary>
    /// A simple text button.
    /// </summary>
    public class TextButton
        : Button
    {
        /// <summary>
        /// Gets the label.
        /// </summary>
        /// <value>The label.</value>
        public Label Label { get; private set; }

        /// <summary>
        /// Gets or sets the font colour.
        /// </summary>
        public Color Colour { get; set; }

        /// <summary>
        /// Gets or sets the highlight font colour.
        /// </summary>
        public Color Highlight { get; set; }

        /// <summary>
        /// Gets or sets the justification.
        /// </summary>
        /// <value>The justification.</value>
        public override Justification Justification
        {
            get { return Label.Justification; }
            set { Label.Justification = value; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TextButton"/> class.
        /// </summary>
        /// <param name="parent">This controls parent control.</param>
        /// <param name="font">The font to use to draw this buttons text value.</param>
        /// <param name="text">The text.</param>
        /// <param name="focusScope">The focus scope.</param>
        public TextButton(Control parent, SpriteFont font, string text)
            : base(parent)
        {
            if (text == null)
                throw new ArgumentNullException("text");

            this.Label = new Label(this, font);
            this.Justification = Justification.Centre;
            this.Label.Text = text;
            this.Colour = Color.White;
            this.Highlight = Color.CornflowerBlue;

            SetSize((int)Label.TextSize.X, (int)Label.TextSize.Y);
            Label.SetPoint(Points.TopLeft, Int2D.Zero);
            Label.SetPoint(Points.TopRight, Int2D.Zero);
        }

        /// <summary>
        /// Updates the control and its' children.
        /// </summary>
        /// <param name="gameTime">The current game time.</param>
        public override void Update(GameTime gameTime)
        {
            var c = IsFocused || IsWarm ? Highlight : Colour;
            Label.Colour = c;
            SetSize((int)Label.TextSize.X, (int)Label.TextSize.Y);
            base.Update(gameTime);
        }
    }
}
