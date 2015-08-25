using System;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Ninject;

using Game = Microsoft.Xna.Framework.Game;
using GameServiceContainer = Microsoft.Xna.Framework.GameServiceContainer;
using GameComponentCollection = Microsoft.Xna.Framework.GameComponentCollection;

namespace Myre.Entities
{
    /// <summary>
    /// Contains a singleton ninject kernel instance.
    /// </summary>
    public static class NinjectKernel
    {
        private static IKernel _kernel;

        /// <summary>
        /// Gets the instance.
        /// </summary>
        public static IKernel Instance
        {
            get
            {
                if (_kernel == null)
                    _kernel = new StandardKernel();

                return _kernel;
            }
        }

        /// <summary>
        /// Binds the game into the kernel.
        /// </summary>
        /// <param name="game">The game.</param>
        /// <param name="bindGraphicsDevice">if set to <c>true</c> binds game.GraphicsDevice.</param>
        /// <param name="bindContentManager">if set to <c>true</c> binds game.Content.</param>
        /// <param name="bindServiceContainer">if set to <c>true</c> binds game.Services.</param>
        public static void BindGame(Game game, bool bindGraphicsDevice = true, bool bindContentManager = true, bool bindServiceContainer = true)
        {
            BindGame(game, Instance, bindGraphicsDevice, bindContentManager, bindServiceContainer);
        }

        /// <summary>
        /// Binds the game into the kernel.
        /// </summary>
        /// <param name="game">The game.</param>
        /// <param name="kernel">The kernel to bind into.</param>
        /// <param name="bindGraphicsDevice">if set to <c>true</c> binds game.GraphicsDevice.</param>
        /// <param name="bindContentManager">if set to <c>true</c> binds game.Content.</param>
        /// <param name="bindServiceContainer">if set to <c>true</c> binds game.Services.</param>
        /// <param name="bindComponentCollection"></param>
        public static void BindGame(Game game, IKernel kernel, bool bindGraphicsDevice = true, bool bindContentManager = true, bool bindServiceContainer = true, bool bindComponentCollection = true)
        {
            // bind the game to a singleton instance
            var thisType = game.GetType();
            kernel.Bind(thisType).ToConstant(game);
            kernel.Bind<Game>().ToConstant(game);

            // bind the graphics device
            if (bindGraphicsDevice)
                kernel.Bind<GraphicsDevice>().ToMethod(c => game.GraphicsDevice);

            // bind the content manager
            if (bindContentManager)
                kernel.Bind<ContentManager>().ToMethod(c => game.Content);

            // bind services
            if (bindServiceContainer)
            {
                kernel.Bind<GameServiceContainer>().ToMethod(c => game.Services);
                kernel.Bind<IServiceProvider>().ToMethod(c => game.Services);
            }

            if (bindComponentCollection)
            {
                kernel.Bind<GameComponentCollection>().ToMethod(c => game.Components);
            }
        }
    }
}
