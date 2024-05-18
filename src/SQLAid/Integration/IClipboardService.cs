using System.Windows.Forms;

namespace SQLAid.Integration
{
    public interface IClipboardService
    {
        void Set(string value);

        string GetFromClipboard();

        void Set(string value, TextDataFormat format);
    }
}