namespace SQLAid.Integration.DTE
{
    public interface IVsStatusBar
    {
        void SetText(string text);

        void Animate(bool Enable);
    }
}