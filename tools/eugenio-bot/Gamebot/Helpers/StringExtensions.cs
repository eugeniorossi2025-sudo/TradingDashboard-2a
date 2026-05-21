using System;

namespace Gamebot.Helpers
{
    public static class StringExtensions
    {
        public static bool Contains(this string source, string toCheck, StringComparison comp)
        {
            return source != null && source.IndexOf(toCheck, comp) >= 0;
        }
    }
}
