
namespace Myre.UI
{
    static class Clipboard
    {
        public static string Text
        {
            get { return System.Windows.Forms.Clipboard.GetText(); }
            set { System.Windows.Forms.Clipboard.SetText(value); }
        }
    }
}
