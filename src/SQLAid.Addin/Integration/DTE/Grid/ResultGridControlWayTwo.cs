using Microsoft.SqlServer.Management.UI.Grid;
using SQLAid.Addin.Extension;

namespace SQLAid.Integration.DTE.Grid
{
    public class ResultGridControlWayTwo : ResultGridControlBase
    {
        private readonly IFrameDocumentView _frameDocumentView;

        public ResultGridControlWayTwo(IFrameDocumentView frameDocumentView)
        {
            _frameDocumentView = frameDocumentView;
        }

        public override IGridControl GetCurrentGridControl()
        {
            var sqlScriptEditorControl = _frameDocumentView.GetCurrentlyActiveFrameDocView();
            var sqlResultsControl = sqlScriptEditorControl.GetField("m_sqlResultsControl");
            var gridResultsPage = sqlResultsControl.GetField("m_gridResultsPage");
            return gridResultsPage.GetProperty<IGridControl>("FocusedGrid");
        }
    }
}