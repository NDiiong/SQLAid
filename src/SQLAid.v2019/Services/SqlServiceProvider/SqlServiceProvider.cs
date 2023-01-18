using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.TextManager.Interop;
using System;

namespace SQLAid.Services.SqlServiceProvider
{
    internal class SqlServiceProvider : ISqlServiceProvider
    {
        public IWpfTextView GetSqlWpfTextView(IServiceProvider serviceProvider)
        {
            IVsTextManager vsTextManager = serviceProvider.GetService(typeof(SVsTextManager)) as IVsTextManager;
            IComponentModel componentModel = serviceProvider.GetService(typeof(SComponentModel)) as IComponentModel;
            IVsEditorAdaptersFactoryService vsEditorAdaptersFactoryService = componentModel.GetService<IVsEditorAdaptersFactoryService>();
            vsTextManager.GetActiveView(1, null, out IVsTextView ppView);
            return vsEditorAdaptersFactoryService.GetWpfTextView(ppView);
        }
    }
}