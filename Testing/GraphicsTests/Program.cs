using System;
using Myre.StateManagement;

namespace GraphicsTests
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            GameLauncher.Run<TestGame>();
        }
    }
}

