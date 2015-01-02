using System.IO;
using System.Text;

namespace Myre.Debugging.UI
{
    internal class ConsoleWriter
        : TextWriter
    {
        private readonly CommandConsole _commandConsole;

        public ConsoleWriter(CommandConsole commandConsole)
        {
            _commandConsole = commandConsole;
        }

        public override Encoding Encoding
        {
            get
            {
                return Encoding.UTF8;
            }
        }

        public override void Write(char value)
        {
            if (char.IsControl(value))
            {
                if (value == '\n')
                    _commandConsole.WriteLine("");
            }
            else
            {
                _commandConsole.Write(value);
            }
        }
    }
}
