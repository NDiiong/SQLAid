using System;
using System.Windows.Forms;

namespace SQLAid.Services.Runtime
{
    internal class ClipboardService : IClipboardService
    {
        public void Set(string value)
        {
            try
            {
                Clipboard.SetText(value, TextDataFormat.UnicodeText);
            }
            catch (Exception)
            {
            }
        }

        public string GetFromClipboard()
        {
            try
            {
                if (Clipboard.ContainsText())
                    return Clipboard.GetText(TextDataFormat.UnicodeText);
            }
            catch (Exception)
            {
            }

            return string.Empty;
        }
    }
}