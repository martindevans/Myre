using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.Xna.Framework;

namespace Myre.UI.Text
{
    public static class ColourParser
    {
        private static Dictionary<StringPart, Color> _colours;

        public static Dictionary<StringPart, Color> Colours
        {
            get 
            {
                if (_colours == null)
                    InitialiseColours();

                return _colours; 
            }
        }

        private static void InitialiseColours()
        {
            _colours = new Dictionary<StringPart, Color>();
            var t = typeof(Color);
            var properties = t.GetProperties(BindingFlags.Public | BindingFlags.Static);
            foreach (var p in properties)
            {
                if (!p.CanRead)
                    continue;

                var value = p.GetValue(null, null);
                if (value.GetType() == t)
                    _colours.Add(p.Name.ToUpper(), (Color)value);
            }
        }

        public static Color Parse(StringPart text)
        {
            Color value;
            if (TryParse(text, out value))
                return value;
            else
                throw new FormatException(text + " could not be parsed into a colour.");
        }

        public static bool TryParse(StringPart text, out Color value)
        {
            if (Colours.TryGetValue(text, out value))
                return true;

            if (TryUppercase(text, out value))
                return true;

            if (TryComponents(text, out value))
                return true;

            return false;
        }

        private static bool TryUppercase(StringPart text, out Color value)
        {
            var name = text.ToString().ToUpper();

            if (Colours.TryGetValue(name, out value))
            {
                Colours.Add(text, value);
                return true;
            }

            return false;
        }

        private static bool TryComponents(StringPart text, out Color value)
        {
            var name = text.ToString().ToUpper();
            var parts = name.Split(new char[] { ' ', ',', ';', ':' });

            if (ReadColourComponents(parts, out value))
            {
                Colours.Add(text, value);
                return true;
            }

            return false;
        }

        private static bool ReadColourComponents(string[] parts, out Color value)
        {
            var components = new byte[4];

            if (parts.Length == 1)
            {
                if (TryParseByte(parts[0], out components[0]))
                {
                    value = new Color(components[0], components[0], components[0]);
                    return true;
                }
            }
            else if (parts.Length == 3)
            {
                if (TryParseByte(parts[0], out components[0]) &&
                    TryParseByte(parts[1], out components[1]) &&
                    TryParseByte(parts[2], out components[2]))
                {
                    value = new Color(components[0], components[1], components[2]);
                    return true;
                }
            }
            else if (parts.Length == 4)
            {
                if (TryParseByte(parts[0], out components[0]) &&
                    TryParseByte(parts[1], out components[1]) &&
                    TryParseByte(parts[2], out components[2]) &&
                    TryParseByte(parts[3], out components[3]))
                {
                    value = new Color(components[0], components[1], components[2], components[3]);
                    return true;
                }
            }

            value = Color.White;
            return false;
        }

        private static bool TryParseByte(string text, out byte value)
        {
            try
            {
                value = byte.Parse(text);
                return true;
            }
            catch (FormatException)
            {
                value = 0;
                return false;
            }
        }
    }
}
