using System;
using Microsoft.Xna.Framework.Input;

namespace Myre.Extensions
{
    /// <summary>
    /// 
    /// </summary>
    public static class KeysExtensions
    {
        /// <summary>
        /// Determines if this key is used in basic character entry.
        /// Includes a-z, 0-9 and arrow keys, among others.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static bool IsCharacterKey(this Keys key)
        {
            switch (key)
            {
                default:
                    throw new ArgumentOutOfRangeException("key", string.Format("Unknown key code {0}", key));

                case Keys.Apps:
                case Keys.Attn:
                case Keys.BrowserBack:
                case Keys.BrowserFavorites:
                case Keys.BrowserForward:
                case Keys.BrowserHome:
                case Keys.BrowserRefresh:
                case Keys.BrowserSearch:
                case Keys.BrowserStop:
                case Keys.ChatPadGreen:
                case Keys.ChatPadOrange:
                case Keys.Crsel:
                case Keys.Enter:
                case Keys.EraseEof:
                case Keys.Escape:
                case Keys.Execute:
                case Keys.Exsel:
                case Keys.F1:
                case Keys.F10:
                case Keys.F11:
                case Keys.F12:
                case Keys.F13:
                case Keys.F14:
                case Keys.F15:
                case Keys.F16:
                case Keys.F17:
                case Keys.F18:
                case Keys.F19:
                case Keys.F2:
                case Keys.F20:
                case Keys.F21:
                case Keys.F22:
                case Keys.F23:
                case Keys.F24:
                case Keys.F3:
                case Keys.F4:
                case Keys.F5:
                case Keys.F6:
                case Keys.F7:
                case Keys.F8:
                case Keys.F9:
                case Keys.Help:
                case Keys.ImeConvert:
                case Keys.ImeNoConvert:
                case Keys.Kana:
                case Keys.Kanji:
                case Keys.LaunchApplication1:
                case Keys.LaunchApplication2:
                case Keys.LaunchMail:
                case Keys.LeftAlt:
                case Keys.LeftControl:
                case Keys.LeftWindows:
                case Keys.MediaNextTrack:
                case Keys.MediaPlayPause:
                case Keys.MediaPreviousTrack:
                case Keys.MediaStop:
                case Keys.None:
                case Keys.Oem8:
                case Keys.OemAuto:
                case Keys.OemClear:
                case Keys.OemCopy:
                case Keys.OemEnlW:
                case Keys.Pa1:
                case Keys.PageDown:
                case Keys.PageUp:
                case Keys.Pause:
                case Keys.Play:
                case Keys.Print:
                case Keys.PrintScreen:
                case Keys.ProcessKey:
                case Keys.RightAlt:
                case Keys.RightControl:
                case Keys.RightWindows:
                case Keys.Scroll:
                case Keys.Select:
                case Keys.SelectMedia:
                case Keys.Separator:
                case Keys.Sleep:
                case Keys.Tab:
                case Keys.VolumeDown:
                case Keys.VolumeMute:
                case Keys.VolumeUp:
                case Keys.Zoom:
                    return false;

                case Keys.A:
                case Keys.Add:
                case Keys.B:
                case Keys.Back:
                case Keys.C:
                case Keys.CapsLock:
                case Keys.D:
                case Keys.D0:
                case Keys.D1:
                case Keys.D2:
                case Keys.D3:
                case Keys.D4:
                case Keys.D5:
                case Keys.D6:
                case Keys.D7:
                case Keys.D8:
                case Keys.D9:
                case Keys.Decimal:
                case Keys.Delete:
                case Keys.Divide:
                case Keys.Down:
                case Keys.E:
                case Keys.End:
                case Keys.F:
                case Keys.G:
                case Keys.H:
                case Keys.Home:
                case Keys.I:
                case Keys.Insert:
                case Keys.J:
                case Keys.K:
                case Keys.L:
                case Keys.Left:
                case Keys.LeftShift:
                case Keys.M:
                case Keys.Multiply:
                case Keys.N:
                case Keys.NumLock:
                case Keys.NumPad0:
                case Keys.NumPad1:
                case Keys.NumPad2:
                case Keys.NumPad3:
                case Keys.NumPad4:
                case Keys.NumPad5:
                case Keys.NumPad6:
                case Keys.NumPad7:
                case Keys.NumPad8:
                case Keys.NumPad9:
                case Keys.O:
                case Keys.OemSemicolon:
                case Keys.OemBackslash:
                case Keys.OemQuestion:
                case Keys.OemTilde:
                case Keys.OemOpenBrackets:
                case Keys.OemPipe:
                case Keys.OemCloseBrackets:
                case Keys.OemQuotes:
                case Keys.OemComma:
                case Keys.OemMinus:
                case Keys.OemPeriod:
                case Keys.OemPlus:
                case Keys.P:
                case Keys.Q:
                case Keys.R:
                case Keys.Right:
                case Keys.RightShift:
                case Keys.S:
                case Keys.Space:
                case Keys.Subtract:
                case Keys.T:
                case Keys.U:
                case Keys.Up:
                case Keys.V:
                case Keys.W:
                case Keys.X:
                case Keys.Y:
                case Keys.Z:
                    return true;
            }
        }
    }
}
