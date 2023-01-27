using System;
using System.ComponentModel;

namespace SQLAid.Addin.Extension
{
    public static class StringExtension
    {
        public static string Escapse(string @value, string oldValue, string newValue)
        {
            return @value.Replace(oldValue, newValue);
        }
    }
}