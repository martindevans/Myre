using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;

namespace Myre.StateManagement
{
    class ExceptionGame : Game
    {
        public const string ERROR_TITLE = "Unexpected Error";
        public const string ERROR_MESSAGE = "The game had an unexpected error and had to shut down.";

        public static readonly string[] ErrorButtons = new[]
        {
            "Exit Game"
        };

        private readonly Exception _exception;
        private bool _shownMessage;

        private SpriteBatch _batch;

        public ExceptionGame(Exception e)
        {
            _exception = e;
            Components.Add(new GamerServicesComponent(this));
        }

        protected override void LoadContent()
        {
            _batch = new SpriteBatch(GraphicsDevice);
        }

        protected override void Update(GameTime gameTime)
        {
            try
            {
                if (!_shownMessage)
                {
                    if (!Guide.IsVisible)
                    {
                        Guide.BeginShowMessageBox(
                            PlayerIndex.One,
                            ERROR_TITLE,
                            ERROR_MESSAGE,
                            ErrorButtons,
                            0,
                            MessageBoxIcon.Error,
                            result => Exit(),
                            null);
                        _shownMessage = true;
                    }
                }
            }
            catch { }

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);
        }
    }
}
