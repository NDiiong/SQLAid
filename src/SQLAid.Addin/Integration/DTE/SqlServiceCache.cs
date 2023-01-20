using EnvDTE80;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.TextManager.Interop;
using SQLAid.Extensions;
using SQLAid.Integration.Clipboard;
using System;

namespace SQLAid.Integration.DTE
{
    public static class SqlServiceCache
    {
        public static IServiceProvider Service { get; }
        public static DTE2 ApplicationObject { get; }
        public static IVsStatusbar Statusbar { get; }
        public static IVsTextManager VsTextManager { get; }
        public static IVsTextManager2 VsTextManager2 { get; }
        public static IVsStatusbar VsStatusbar { get; }
        public static IVsRunningDocumentTable VsRunningDocumentTable { get; }
        public static IVsFontAndColorStorage VsFontAndColorStorage { get; }
        public static IVsHiddenTextManager VsHiddenTextManager { get; }
        public static IClipboardService ClipboardService { get; }
        public static IVsMonitorSelection VsMonitorSelection { get; }

        static SqlServiceCache()
        {
            ClipboardService = new ClipboardService();

            ApplicationObject = Package.GetGlobalService(typeof(EnvDTE.DTE)) as DTE2;
            Service = new ServiceProvider(ApplicationObject as Microsoft.VisualStudio.OLE.Interop.IServiceProvider);
            Statusbar = Package.GetGlobalService(typeof(SVsStatusbar)).As<IVsStatusbar>();
            VsTextManager = Service.GetService(typeof(SVsTextManager)).As<IVsTextManager>();
            VsTextManager2 = Service.GetService(typeof(SVsTextManager)).As<IVsTextManager2>();
            VsStatusbar = Service.GetService(typeof(SVsStatusbar)).As<IVsStatusbar>();
            VsRunningDocumentTable = Service.GetService(typeof(SVsRunningDocumentTable)).As<IVsRunningDocumentTable>();
            VsFontAndColorStorage = Service.GetService(typeof(SVsFontAndColorStorage)).As<IVsFontAndColorStorage>();
            VsHiddenTextManager = Service.GetService(typeof(SVsTextManager)).As<IVsHiddenTextManager>();
            VsMonitorSelection = Service.GetService(typeof(IVsMonitorSelection)).As<IVsMonitorSelection>();
        }
    }
}