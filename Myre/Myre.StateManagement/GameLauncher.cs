using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using System.Diagnostics;

namespace Myre.StateManagement
{
    public static class GameLauncher
    {
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
                    using (var blackbox = System.IO.File.CreateText("BLACKBOX " + DateTime.Now.Day + " " + DateTime.Now.Month + " " + DateTime.Now.Year + ".txt"))
                    {
                        blackbox.Write(e.ToString());

                        if (e.InnerException != null)
                            blackbox.WriteLine(e.InnerException.ToString());
                    }

                    if (System.Windows.Forms.MessageBox.Show(ExceptionGame.ErrorMessage + "\nDo you wish to view error details?", ExceptionGame.ErrorTitle, System.Windows.Forms.MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.Yes)
                    {
                        System.Windows.Forms.MessageBox.Show(e.ToString());
                    }
#endif
                }
            }
        }
    }
}
