using System;
using System.IO;
using System.Windows.Forms;

using Color = Microsoft.Xna.Framework.Color;
using Game = Microsoft.Xna.Framework.Game;
using GameTime = Microsoft.Xna.Framework.GameTime;
using GraphicsDeviceManager = Microsoft.Xna.Framework.GraphicsDeviceManager;

namespace ContentBuilderGame
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Game
    {
        GraphicsDeviceManager _graphics;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            MessageBox.Show("The only function of this project is to trigger rebuild of Myre.Graphics.Content");

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            base.LoadContent();

            //Print out all built content names (prints out exactly the string you need to pass into content.Load)
            var files = Directory.GetFiles(Content.RootDirectory, "*.xnb", SearchOption.AllDirectories);
            foreach (var file in files)
            {
                var f = file;
                if (f.StartsWith(Content.RootDirectory + Path.DirectorySeparatorChar))
                    f = f.Remove(0, Content.RootDirectory.Length + 1);

                if (f.EndsWith(".xnb"))
                    f = f.Remove(f.Length - 4, 4);

                Console.WriteLine(f);
            }

            Exit();
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            base.Draw(gameTime);
        }
    }
}
