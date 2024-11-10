using Microsoft.SqlServer.Management.UI.VSIntegration;
using Microsoft.SqlServer.Management.UI.VSIntegration.Editors;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;
using SQLAid.Extensions;

namespace SQLAid.Integration.DTE.Grid
{
    public class ResultMessagesControl : IResultMessagesControl
    {
        public string GetMessages()
        {
            object outVsWindowFrame = null;
            ServiceCache.VSMonitorSelection.GetCurrentElementValue((int)VSConstants.VSSELELEMID.SEID_WindowFrame, ref outVsWindowFrame);

            var vsWindowFrame = outVsWindowFrame as IVsWindowFrame;
            vsWindowFrame.GetProperty((int)__VSFPROPID.VSFPROPID_DocView, out var outControl);

            var editorControl = outControl.As<SqlScriptEditorControl>();

            dynamic controlBelowSplitter = Reflection.GetField(editorControl, "controlBelowSplitter");
            var shellTextViewControl = ((object)controlBelowSplitter.ActiveControl).As<ShellTextViewControl>();
            var textView = shellTextViewControl.TextView;

            textView.GetBuffer(out var lines);
            lines.GetLineCount(out var lineCount);
            lines.GetLastLineIndex(out var lastLine, out var lastLineIndex);
            lines.GetLengthOfLine(0, out var lineLength);
            lines.GetLineText(0, 0, lastLine, lastLineIndex, out var textBuffer);

            return textBuffer;
        }
    }
}