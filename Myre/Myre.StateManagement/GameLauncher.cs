using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using Microsoft.Xna.Framework;
using System.Diagnostics;
using System.Net.Mail;

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
            if (Debugger.IsAttached)
            {
                using (var g = new T())
                    g.Run();
            }
            else
            {
                try
                {
                    using (var g = new T())
                        g.Run();
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
