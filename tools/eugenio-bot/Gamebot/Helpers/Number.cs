using System;

namespace Gamebot.Helpers
{
    internal class Number
    {
        public static string FormatNumberDecimalEuro(float number)
        {
            return Math.Round((double)number, 2).ToString() + "€";
        }

        public static string FormatNumberDecimalEuro(decimal number)
        {
            return Math.Round(number, 2).ToString() + "€";
        }

        public static string FormatNumberDecimalEuro(double number)
        {
            return Math.Round(number, 2).ToString() + "€";
        }
    }
}
