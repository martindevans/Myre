using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Xna.Framework;

namespace Myre.StateManagement
{
    public static class GameLauncher
    {
#if WINDOWS
        static readonly List<Action<Exception>> _exceptionActions = new List<Action<Exception>>();
        public static void AddExceptionAction(Action<Exception> action)
        {
            _exceptionActions.Add(action);
        }
#endif

        public static void Run<T>() where T : Game, new()
        {
            Run<T>(new T());
        }

        public static void Run<T>(T instance) where T : Game
        {
            if (Debugger.IsAttached)
            {
                using (instance)
                    instance.Run();
            }
            else
            {
                try
                {
                    using (instance)
                        instance.Run();
                }
                catch (Exception e)
                {
#if !WINDOWS
                    using (var g = new ExceptionGame(e))
                        g.Run();
#else
                    foreach (Action<Exception> t in _exceptionActions)
                    {
                        t(e);
                    }
#endif
                }
            }
        }
    }
}
