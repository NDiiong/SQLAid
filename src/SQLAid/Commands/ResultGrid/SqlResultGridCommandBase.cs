#pragma warning disable IDE1006

using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.CommandBars;
using Microsoft.VisualStudio.Shell;
using SQLAid.Integration;
using SQLAid.Integration.DTE.Grid;
using System;

namespace SQLAid.Commands.ResultGrid
{
    internal abstract class SqlResultGridCommandBase
    {
        private const string SQL_RESULT_GRID_CONTEXT_NAME = "SQL Results Grid Tab Context";
        private static CommandBar commandBar { get; }

        protected static CommandBarControl SqlAidGridControl;

        protected static readonly IResultGridControl GridControl;

        static SqlResultGridCommandBase()
        {
            GridControl = new ResultGridControl();
            var dte = Package.GetGlobalService(typeof(DTE)) as DTE2;
            commandBar = ((CommandBars)dte.CommandBars)[SQL_RESULT_GRID_CONTEXT_NAME];

            SqlAidGridControl = commandBar.Controls.Add(MsoControlType.msoControlPopup, 1, Type.Missing, Type.Missing, false);
            SqlAidGridControl.Caption = "SQL Aid";
            SqlAidGridControl.Enabled = SqlAidGridControl.Visible = true;
            SqlAidGridControl.TooltipText = "SQLAid - Tools for SQL Server Management Studio";
        }
    }
}