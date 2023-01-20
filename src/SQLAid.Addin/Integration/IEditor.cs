using System.Collections.Specialized;

namespace SQLAid.Integration
{
    public interface IEditor
    {
        EditedLine GetEditedLine();

        void SetContent(string content);

        void SetContent(StringCollection content);

        string Sanitize(string content, string columns);

        void SetContent(string text, int count);
    }
}