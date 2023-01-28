#pragma warning disable IDE1006

using Microsoft.VisualStudio.CommandBars;
using Microsoft.VisualStudio.Shell;
using SQLAid.Addin.Extension;
using SQLAid.Helpers;
using SQLAid.Integration;
using SQLAid.Integration.Clipboard;
using SQLAid.Integration.DTE;
using SQLAid.Integration.DTE.Commandbars;
using SQLAid.Integration.DTE.Grid;
using System;
using System.Collections.Generic;
using System.IO;
using Task = System.Threading.Tasks.Task;

namespace SQLAid.Commands.Grid
{
    internal sealed class SqlResultGridAsCsharpClassCommand : SqlResultGridCommandBase
    {
        private static CommandBarButton _commandBarButton;
        private static readonly IClipboardService _clipboardService;

        static SqlResultGridAsCsharpClassCommand()
        {
            _clipboardService = new ClipboardService();
        }

        public static async Task InitializeAsync(SqlAsyncPackage sqlAsyncPackage)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            _commandBarButton = SqlAidGridControl.As<CommandBarPopup>().Controls
                .Add(MsoControlType.msoControlButton, 1, Type.Missing, Type.Missing, false)
                .Visible(true).Caption("Copy As C# Class")
                .As<CommandBarButton>();

            _commandBarButton
                .AddIcon($"{sqlAsyncPackage.InstallationDirectory}/Assets/c-sharp.ico")
                .AddStyle(MsoButtonStyle.msoButtonIconAndCaption)
                .Click += (CommandBarButton _, ref bool __) => CsharpClassEventHandle(sqlAsyncPackage);
        }

        private static void CsharpClassEventHandle(SqlAsyncPackage sqlAsyncPackage)
        {
            var currentGridControl = GridControl.GetCurrentGridControl();
            if (currentGridControl != null)
            {
                using (var gridResultControl = new ResultGridControlAdaptor(currentGridControl))
                {
                    var templates = File.ReadAllText($"{sqlAsyncPackage.InstallationDirectory}/Templates/CSHARP.CLASS.tt");
                    var propertySyntaxes = new List<string>();
                    foreach (var (type, propertyName, nullable) in gridResultControl.GetColumnTypes())
                    {
                        var typeName = TypeHelper.Primitive(type);

                        var propertySyntax = nullable
                            ? $"\t\tpublic {typeName}? {propertyName} {{ get; set; }}"
                            : $"\t\tpublic {typeName} {propertyName} {{ get; set; }}";

                        propertySyntaxes.Add(propertySyntax);
                    }

                    var @class = string.Join(Environment.NewLine, propertySyntaxes);
                    templates = templates.Replace("{properties}", @class);
                    _clipboardService.Set(templates);
                }
            }
        }
    }
}