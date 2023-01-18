namespace SQLAid.Services.Runtime
{
    public interface IClipboardService
    {
        void Set(string @value);
        string GetFromClipboard();
    }
}