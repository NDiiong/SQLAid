using Microsoft.VisualStudio.Text.Editor;
using System;

namespace SQLAid.Services.SqlServiceProvider
{
    internal interface ISqlServiceProvider
    {
        IWpfTextView GetSqlWpfTextView(IServiceProvider serviceProvider);
    }
}