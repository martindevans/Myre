using System;
using Microsoft.Xna.Framework.Graphics;

using Rectangle = Microsoft.Xna.Framework.Rectangle;
using Color = Microsoft.Xna.Framework.Color;

namespace Myre.UI.Controls
{
    /// <summary>
    /// A control which draws an image.
    /// </summary>
    public class ImageBox
        : Control
    {
        private Texture2D _texture;

        /// <summary>
        /// Gets or sets the texture.
        /// </summary>
        /// <value>The texture.</value>
        public Texture2D Texture 
        {
            get { return _texture; }
            set
            {
                if (value == null)
                    throw new ArgumentNullException("value");
                _texture = value;
            }
        }

        /// <summary>
        /// Gets or sets the source rectangle.
        /// </summary>
        /// <value>The source rectangle.</value>
        public Rectangle? SourceRectangle { get; set; }

        /// <summary>
        /// Gets or sets the colour.
        /// </summary>
        /// <value>The colour.</value>
        public Color Colour { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ImageBox"/> class.
        /// </summary>
        /// <param name="parent">This controls parent control.</param>
        /// <param name="texture">The texture.</param>
        /// <param name="colour">The colour.</param>
        /// <param name="sourceRectangle">The source rectangle.</param>
        public ImageBox(Control parent, Texture2D texture, Color colour, Rectangle? sourceRectangle)
            : base(parent)
        {
            Texture = texture;
            SourceRectangle = sourceRectangle;
            Colour = colour;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ImageBox"/> class.
        /// </summary>
        /// <param name="parent">This controls parent control.</param>
        /// <param name="texture">The texture.</param>
        public ImageBox(Control parent, Texture2D texture)
            : this(parent, texture, Color.White, null)
        {
        }

        /// <summary>
        /// Draws the control.
        /// </summary>
        /// <param name="batch">An spritebactch already started for alpha blending with deferred sort mode.</param>
        public override void Draw(SpriteBatch batch)
        {
            batch.Draw(
                _texture,
                Area,
                SourceRectangle,
                Colour);
        }
    }
}
