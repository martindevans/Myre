// http://www.gamedev.net/community/forums/topic.asp?topic_id=457783

using System;
using System.Runtime.InteropServices;
using Microsoft.Xna.Framework.Input;

namespace Myre.UI.InputDevices
{
    public class CharacterEventArgs : EventArgs
    {
        private readonly char _character;
        private readonly int _lParam;

        public CharacterEventArgs(char character, int lParam)
        {
            _character = character;
            _lParam = lParam;
        }

        public char Character
        {
            get { return _character; }
        }

        public int Param
        {
            get { return _lParam; }
        }

        public int RepeatCount
        {
            get { return _lParam & 0xffff; }
        }

        public bool ExtendedKey
        {
            get { return (_lParam & (1 << 24)) > 0; }
        }

        public bool AltPressed
        {
            get { return (_lParam & (1 << 29)) > 0; }
        }

        public bool PreviousState
        {
            get { return (_lParam & (1 << 30)) > 0; }
        }

        public bool TransitionState
        {
            get { return (_lParam & (1 << 31)) > 0; }
        }
    }

    public class KeyEventArgs : EventArgs
    {
        private readonly Keys _keyCode;

        public KeyEventArgs(Keys keyCode)
        {
            _keyCode = keyCode;
        }

        public Keys KeyCode
        {
            get { return _keyCode; }
        }
    }

    public delegate void CharEnteredHandler(object sender, CharacterEventArgs e);
    public delegate void KeyEventHandler(object sender, KeyEventArgs e);

    /// <summary>
    /// On Windows, this class hooks into windows events, and allows interpretation of WM_CHAR events.
    /// </summary>
    public static class TextInput
    {
        /// <summary>
        /// Event raised when a character has been entered.
        /// Only called on Windows.
        /// </summary>
        public static event CharEnteredHandler CharEntered;

        /// <summary>
        /// Event raised when a key has been pressed down. May fire multiple times due to keyboard repeat.
        /// Only called on Windows.
        /// </summary>
        public static event KeyEventHandler KeyDown;

        /// <summary>
        /// Event raised when a key has been released.
        /// Only called on Windows.
        /// </summary>
        public static event KeyEventHandler KeyUp;

        public static bool Initialized
        {
            get;
            private set;
        }

        delegate IntPtr WndProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

        static IntPtr _prevWndProc;
        static WndProc _hookProcDelegate;
        static IntPtr _hImc;

        //various Win32 constants that we need
        const int GWL_WNDPROC = -4;
        const int WM_KEYDOWN = 0x100;
        const int WM_KEYUP = 0x101;
        const int WM_CHAR = 0x102;
        const int WM_IME_SETCONTEXT = 0x0281;
        const int WM_INPUTLANGCHANGE = 0x51;
        const int WM_GETDLGCODE = 0x87;
        const int WM_IME_COMPOSITION = 0x10f;
        const int DLGC_WANTALLKEYS = 4;

        //Win32 functions that we're using
        [DllImport("Imm32.dll")]
        static extern IntPtr ImmGetContext(IntPtr hWnd);

        [DllImport("Imm32.dll")]
        static extern IntPtr ImmAssociateContext(IntPtr hWnd, IntPtr hImc);

        [DllImport("user32.dll")]
        static extern IntPtr CallWindowProc(IntPtr lpPrevWndFunc, IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll")]
        static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        /// <summary>
        /// Initialize the TextInput with the given GameWindow.
        /// </summary>
        /// <param name="handle">The XNA window to which text input should be linked.</param>
        public static void Initialize(IntPtr handle)
        {
            if (Initialized)
                throw new InvalidOperationException("TextInput.Initialize can only be called once!");

            _hookProcDelegate = HookProc;
            _prevWndProc = (IntPtr)SetWindowLong(handle, GWL_WNDPROC,
                (int)Marshal.GetFunctionPointerForDelegate(_hookProcDelegate));

            _hImc = ImmGetContext(handle);
            Initialized = true;
        }

        static IntPtr HookProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam)
        {
            IntPtr returnCode = CallWindowProc(_prevWndProc, hWnd, msg, wParam, lParam);

            switch (msg)
            {
                case WM_GETDLGCODE:
                    returnCode = (IntPtr)(returnCode.ToInt32() | DLGC_WANTALLKEYS);
                    break;

                case WM_KEYDOWN:
                    if (KeyDown != null)
                        KeyDown(null, new KeyEventArgs((Keys)wParam));
                    break;

                case WM_KEYUP:
                    if (KeyUp != null)
                        KeyUp(null, new KeyEventArgs((Keys)wParam));
                    break;

                case WM_CHAR:
                    if (CharEntered != null)
                        CharEntered(null, new CharacterEventArgs((char)wParam, lParam.ToInt32()));
                    break;

                case WM_IME_SETCONTEXT:
                    if (wParam.ToInt32() == 1)
                        ImmAssociateContext(hWnd, _hImc);
                    break;

                case WM_INPUTLANGCHANGE:
                    ImmAssociateContext(hWnd, _hImc);
                    returnCode = (IntPtr)1;
                    break;
            }

            return returnCode;
        }
    }
}