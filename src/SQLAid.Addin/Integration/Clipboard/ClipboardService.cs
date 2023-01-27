using SQLAid.Addin.Logging;
using System;
using TextDataFormat = System.Windows.Forms.TextDataFormat;

namespace SQLAid.Integration.Clipboard
{
    internal class ClipboardService : IClipboardService
    {
        public void SetDataObject(object @object)
        {
            Func.Ignore<Exception>(() => System.Windows.Forms.Clipboard.SetDataObject(@object, true));
        }

        public void Set(string value)
        {
            Func.Ignore<Exception>(() => System.Windows.Forms.Clipboard.SetText(value));
        }

        public string GetFromClipboard()
        {
            if (System.Windows.Forms.Clipboard.ContainsText())
                return System.Windows.Forms.Clipboard.GetText(TextDataFormat.UnicodeText);

            return string.Empty;
        }
    }
}