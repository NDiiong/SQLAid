using System;
using System.Windows;

namespace SQLAid.Integration.Clipboard
{
    internal class ClipboardService : IClipboardService
    {
        public void Set(string value)
        {
            try
            {
                System.Windows.Clipboard.SetText(value, TextDataFormat.UnicodeText);
            }
            catch (Exception)
            {
            }
        }

        public string GetFromClipboard()
        {
            try
            {
                if (System.Windows.Clipboard.ContainsText())
                    return System.Windows.Clipboard.GetText(TextDataFormat.UnicodeText);
            }
            catch (Exception)
            {
            }

            return string.Empty;
        }
    }
}