using System;

namespace SQLAid.Extensions
{
    public static class StringExtension
    {
        public static string Escapse(string @value, string oldValue, string newValue)
        {
            return @value.Replace(oldValue, newValue);
        }

        public static bool Contains(this string source, string toCheck, StringComparison comp)
        {
            if (string.IsNullOrEmpty(toCheck) || string.IsNullOrEmpty(source))
                return true;

            return source.IndexOf(toCheck, comp) >= 0;
        }
    }
}