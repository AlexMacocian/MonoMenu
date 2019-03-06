using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MonoMenu.Helper
{
    class HelperFunctions
    {
        public static Exception InvalidColor = new Exception("String could not be parsed into a valid color");

        public static Random Random = new Random();

        public static Color ColorFromString(string colorcode)
        {
            if(colorcode[0] == '#')
            {
                return ColorFromHex(colorcode);
            }
            else
            {
                Type t = typeof(Color);
                try
                {
                    Color c = (Color)t.GetProperty(colorcode, BindingFlags.Static | BindingFlags.Public).GetValue(null);
                    return c;
                }
                catch
                {
                    throw InvalidColor;
                }
            }
        }

        public static Color ColorFromHex(string colorcode)
        {
            if (colorcode[0] == '#')
            {
                colorcode = colorcode.Remove(0, 1);
            }
            if (colorcode.Length == 9)
            {
                return Color.FromNonPremultiplied(int.Parse(colorcode.Substring(2, 2), NumberStyles.HexNumber),
                int.Parse(colorcode.Substring(4, 2), NumberStyles.HexNumber),
                int.Parse(colorcode.Substring(6, 2), NumberStyles.HexNumber),
                int.Parse(colorcode.Substring(0, 2), NumberStyles.HexNumber));
            }
            else
            {
                return Color.FromNonPremultiplied(
                int.Parse(colorcode.Substring(0, 2), NumberStyles.HexNumber),
                int.Parse(colorcode.Substring(2, 2), NumberStyles.HexNumber),
                int.Parse(colorcode.Substring(4, 2), NumberStyles.HexNumber), 255);
            }
        }

        public static string GenerateString(int length)
        {

            string characters = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";
            StringBuilder result = new StringBuilder(length);
            for (int i = 0; i < length; i++)
            {
                result.Append(characters[Random.Next(characters.Length)]);
            }
            return result.ToString();
        }
    }
}
