#pragma warning disable IDE1006

using Microsoft.VisualStudio.CommandBars;
using SQLAid.Integration.DTE.Commandbars;
using System;

namespace SQLAid.Extensions
{
    public static class CommandBarExtension
    {
        public static CommandBarButton AddButton(this CommandBar CommandBar, string caption, string icon, MsoButtonStyle msoButtonStyle, Action onclick)
        {
            var commandBarButton = CommandBar.Controls.Add(MsoControlType.msoControlButton, Type.Missing, Type.Missing, Type.Missing, true)
                  .Visible(true).Caption(caption)
                  .As<CommandBarButton>()
                  .AddIcon(icon)
                  .AddStyle(msoButtonStyle);
            commandBarButton.Click += (CommandBarButton _, ref bool __) => onclick();

            return commandBarButton;
        }

        public static void AddButton(this CommandBar CommandBar, string caption, Action onclick)
        {
            CommandBar.Controls.Add(MsoControlType.msoControlButton, Type.Missing, Type.Missing, Type.Missing, true)
                .Visible(true).Caption(caption)
                .As<CommandBarButton>()
                .Click += (CommandBarButton _, ref bool __) => onclick();
        }

        public static void AddButton(this CommandBar CommandBar, string caption, string icon, Action onclick)
        {
            CommandBar.Controls.Add(MsoControlType.msoControlButton, Type.Missing, Type.Missing, Type.Missing, true)
                .Visible(true).Caption(caption)
                .As<CommandBarButton>()
                .AddIcon(icon)
                .Click += (CommandBarButton _, ref bool __) => onclick();
        }
    }
}