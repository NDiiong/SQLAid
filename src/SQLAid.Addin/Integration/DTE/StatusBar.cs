using Microsoft.VisualStudio.Shell.Interop;
using SQLAid.Extensions;

namespace SQLAid.Integration.DTE
{
    public static class StatusBar
    {
        private static readonly IVsStatusbar statusBar;

        static StatusBar()
        {
            statusBar = SqlServiceCache.Service.GetService(typeof(SVsStatusbar)).As<IVsStatusbar>();
        }

        public static void SetText(string text)
        {
            int frozen;
            statusBar.IsFrozen(out frozen);

            if (frozen == 0)
            {
                statusBar.SetColorText(text, 0, 0);
            }
        }

        public static void Animate(bool Enable)
        {
            object icon = (short)Constants.SBAI_General;
            statusBar.Animation(Enable ? 1 : 0, ref icon);
        }
    }
}