using Microsoft.SqlServer.Management.UI.Grid;
using SQLAid.Addin.Extension;
using System;
using System.Windows.Forms;

namespace SQLAid.Integration.DTE.Grid
{
    public class ResultGridControlWayFour : ResultGridControlBase
    {
        private readonly IFrameDocumentView _frameDocumentView;

        public ResultGridControlWayFour(IFrameDocumentView frameDocumentView)
        {
            _frameDocumentView = frameDocumentView;
        }

        public override IGridControl GetCurrentGridControl()
        {
            var sqlScriptEditorControl = _frameDocumentView.GetCurrentlyActiveFrameDocView();
            var resultsControl = sqlScriptEditorControl.GetField("m_sqlResultsControl");
            var resultsTabPage = resultsControl.GetField("m_gridResultsPage");
            var controlCollection = resultsTabPage.GetProperty<Control.ControlCollection>("Controls");
            return GetSpecificTypeControl<GridControl>(controlCollection);
        }

        private T GetSpecificTypeControl<T>(Control.ControlCollection controlCollection) where T : Control
        {
            T @value = null;
            var item = 0;
            for (; item < controlCollection.Count; item++)
            {
                if (@value != null)
                    break;

                try
                {
                    @value = controlCollection[item] as T;
                }
                catch (Exception)
                {
                }

                if (controlCollection[item].HasChildren && @value == null)
                    @value = GetSpecificTypeControl<T>(controlCollection[item].Controls);
            }

            return @value;
        }
    }
}