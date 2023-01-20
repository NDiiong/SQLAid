namespace SQLAid.Integration
{
    public interface IClipboardService
    {
        void Set(string @value);
        string GetFromClipboard();
    }
}