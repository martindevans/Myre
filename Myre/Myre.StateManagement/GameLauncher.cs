using System;
using System.Diagnostics;
using Microsoft.Xna.Framework;

namespace Myre.StateManagement
{
    public static class GameLauncher
    {
        public static void Run<T>() where T : Game, new()
        {
            Run(() => new GameWrapper(new T()));
        }

        public static void Run(Game instance)
        {
            Run(() => new GameWrapper(instance));
        }

        public static void Run(Func<ILaunchable> createGame, params Action<Exception>[] exceptionHandlers)
        {
            if (Debugger.IsAttached)
            {
                Trace.TraceInformation("Debugger is attached, running with no global exception handler");

                var launchable = createGame();
                using (launchable)
                    launchable.Run();
            }
            else
            {
                Trace.TraceInformation("No debugger is attached, running with global exception handler");

                try
                {
                    var launchable = createGame();
                    using (launchable)
                        launchable.Run();
                }
                catch (Exception e)
                {
                    foreach (Action<Exception> t in exceptionHandlers)
                    {
                        t(e);
                    }
                }
            }
        }

        private class GameWrapper
            :ILaunchable
        {
            private readonly Game _game;

            public GameWrapper(Game game)
            {
                _game = game;
            }

            public void Run()
            {
                _game.Run();
            }

            public void Dispose()
            {
                _game.Dispose();
            }
        }
    }

    public interface ILaunchable
            : IDisposable
    {
        void Run();
    }
}
