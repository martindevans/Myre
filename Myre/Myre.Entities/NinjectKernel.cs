using System;
using System.Diagnostics.Contracts;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Ninject;
using Ninject.Syntax;
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
                Contract.Ensures(Contract.Result<IKernel>() != null);

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
            Contract.Requires(game != null);

            BindGame(game, Instance, bindGraphicsDevice, bindContentManager, bindServiceContainer);
        }

        private static IBindingToSyntax<T> Bind<T>(IKernel kernel)
        {
            Contract.Requires(kernel != null);
            Contract.Ensures(Contract.Result<IBindingToSyntax<T>>() != null);

            var b = kernel.Bind<T>();
            Contract.Assume(b != null);
            return b;
        }

        private static IBindingToSyntax<object> Bind(IKernel kernel, params Type[] types)
        {
            Contract.Requires(kernel != null);
            Contract.Ensures(Contract.Result<IBindingToSyntax<object>>() != null);

            var b = kernel.Bind(types);
            Contract.Assume(b != null);
            return b;
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
            Contract.Requires(game != null);
            Contract.Requires(kernel != null);

            // bind the game to a singleton instance
            var thisType = game.GetType();
            Bind(kernel, thisType).ToConstant(game);
            Bind<Game>(kernel).ToConstant(game);

            // bind the graphics device
            if (bindGraphicsDevice)
                Bind<GraphicsDevice>(kernel).ToMethod(c => game.GraphicsDevice);

            // bind the content manager
            if (bindContentManager)
                Bind<ContentManager>(kernel).ToMethod(c => game.Content);

            // bind services
            if (bindServiceContainer)
            {
                Bind<GameServiceContainer>(kernel).ToMethod(c => game.Services);
                Bind<IServiceProvider>(kernel).ToMethod(c => game.Services);
            }

            if (bindComponentCollection)
            {
                Bind<GameComponentCollection>(kernel).ToMethod(c => game.Components);
            }
        }
    }
}
