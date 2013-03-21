using System.Text;

namespace Myre.Serialisation
{
    public static class StringEscapes
    {
        public static string Unescape(this string value)
        {
            if (string.IsNullOrEmpty(value)) 
                return value;
            
            StringBuilder sb = new StringBuilder(value.Length);
            for (int i = 0; i < value.Length; )
            {
                int backslash = value.IndexOf('\\', i);
                if (backslash < 0 || backslash == value.Length - 1)
                    backslash = value.Length;
                
                sb.Append(value, i, backslash - i);
                if (backslash >= value.Length)
                    break;
                
                switch (value[backslash + 1])
                {
                    case 'n': sb.Append('\n'); break;  // Line feed
                    case 'r': sb.Append('\r'); break;  // Carriage return
                    case 't': sb.Append('\t'); break;  // Tab
                    case '"': sb.Append('"'); break;
                    case '\\': sb.Append('\\'); break; // Don't escape
                    default:                                 // Unrecognized, copy as-is
                        sb.Append('\\').Append(value[backslash + 1]); break;
                }

                i = backslash + 2;
            }
            
            return sb.ToString();
        }

        public static string Escape(this string value)
        {
            if (string.IsNullOrEmpty(value))
                return value;

            StringBuilder sb = new StringBuilder(value.Length);
            for (int i = 0; i < value.Length; i++)
            {
                switch (value[i])
                {
                    case '\n': sb.Append(@"\n"); break;
                    case '\r': sb.Append(@"\r"); break;
                    case '\t': sb.Append(@"\t"); break;
                    case '"': sb.Append("\\\""); break;
                    default: sb.Append(value[i]); break;
                }
            }

            return sb.ToString();
        }
    }
}
