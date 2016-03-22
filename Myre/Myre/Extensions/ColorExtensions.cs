using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Reflection;

using Color = Microsoft.Xna.Framework.Color;

namespace Myre.Extensions
{
    /// <summary>
    /// A static class containing extension methods for the Microsoft.Xna.Framework.Graphics.Color struct.
    /// </summary>
    public static class ColorExtensions
    {
        private static Dictionary<string, Color> _colours;

        /// <summary>
        /// Multiplies the the specified <see cref="Color"/> with this <see cref="Color"/>.
        /// </summary>
        /// <param name="a">A.</param>
        /// <param name="colour">The colour.</param>
        /// <returns>This <see cref="Color"/> multiplied with the specified <see cref="Color"/>.</returns>
        public static Color Multiply(this Color a, Color colour)
        {
            var aVec = a.ToVector4();
            var bVec = colour.ToVector4();
            return new Color(aVec * bVec);
        }

        /// <summary>
        /// Parses this string into a Color.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="colour">The parsed colour.</param>
        /// <returns>White if not found, the value otherwise</returns>
        public static bool ToColour(this string value, out Color colour)
        {
            if (string.IsNullOrEmpty(value))
                throw new ArgumentNullException("value");

            if (TryGetColourByName(value, out colour))
                return true;

            string[] values = value.Split(new char[] { ' ', ',', ';', ':' });

            byte[] components = new byte[4];

            if (values.Length == 1)
            {
                if (values[0].TryToByte(out components[0]))
                {
                    colour = new Color(components[0], components[0], components[0]);
                    return true;
                }
                
                if (values[0].StartsWith("0x", StringComparison.Ordinal))
                {
                    values[0] = values[0].Remove(0, 2);

                    if (values[0].Length == 6)
                        colour = values[0].FromRgb();
                    else
                        colour = values[0].FromArgb();
                    return true;
                }
            }
            else if (values.Length == 3)
            {
                if (values[0].TryToByte(out components[0]) &&
                    values[1].TryToByte(out components[1]) &&
                    values[2].TryToByte(out components[2]))
                {
                    colour = new Color(components[0], components[1], components[2]);
                    return true;
                }
            }
            else if (values.Length == 4)
            {
                if (values[0].TryToByte(out components[0]) &&
                    values[1].TryToByte(out components[1]) &&
                    values[2].TryToByte(out components[2]) &&
                    values[3].TryToByte(out components[3]))
                {
                    colour = new Color(components[0], components[1], components[2], components[3]);
                    return true;
                }
            }

            colour = Color.White;
            return false;
        }

        private static void InitialiseColours()
        {
            _colours = new Dictionary<string, Color>();
            var t = typeof(Color);
            var properties = t.GetProperties(BindingFlags.Public | BindingFlags.Static);
            foreach (var p in properties)
            {
                if (!p.CanRead)
                    continue;

                var value = p.GetValue(null, null);
                Contract.Assume(value != null);
                if (value.GetType() == t)
                    _colours.Add(p.Name.ToUpper(), (Color)value);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <returns>Null if not found, the value otherwise</returns>
        public static Color FromRgb(this string value)
        {
            uint val = Convert.ToUInt32(value, 16);

            Color r = Color.White;
            r.R = (byte)(val >> 16);
            r.G = (byte)(val >> 8);
            r.B = (byte)val;

            return r;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <returns>Null if not found, the value otherwise</returns>
        public static Color FromArgb(this string value)
        {
            uint val = Convert.ToUInt32(value, 16);

            Color r = Color.White;
            r.A = (byte)(val >> 24);
            r.R = (byte)(val >> 16);
            r.G = (byte)(val >> 8);
            r.B = (byte)val;

            return r;
        }

        private static bool TryGetColourByName(this string name, out Color colour)
        {
            Contract.Requires(name != null);

            if (_colours == null)
                InitialiseColours();

            name = name.ToUpper();
            Debug.Assert(_colours != null, "_colours != null");
            if (!_colours.ContainsKey(name))
            {
                colour = Color.White;
                return false;
            }

            colour = _colours[name];
            return true;
        }
    }
}
