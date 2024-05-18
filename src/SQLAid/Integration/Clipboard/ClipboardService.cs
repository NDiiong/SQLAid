using SQLAid.Logging;
using System;
using System.Windows.Forms;
using Copy = System.Windows.Forms.Clipboard;

namespace SQLAid.Integration.Clipboard
{
    internal class ClipboardService : IClipboardService
    {
        public void SetDataObject(object @object)
        {
            Func.Ignore<Exception>(() => Copy.SetDataObject(@object, true));
        }

        public void Set(string value)
        {
            Func.Ignore<Exception>(() => Copy.SetText(value));
        }

        public string GetFromClipboard()
        {
            return Copy.ContainsText() ? Copy.GetText(TextDataFormat.UnicodeText) : string.Empty;
        }

        public void Set(string value, TextDataFormat html)
        {
            Func.Ignore<Exception>(() => Copy.SetText(value, html));
        }
    }
}