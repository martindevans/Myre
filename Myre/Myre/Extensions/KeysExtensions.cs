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

                default:
                    return true;
            }
        }
    }
}
