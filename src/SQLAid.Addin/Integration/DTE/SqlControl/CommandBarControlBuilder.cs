#pragma warning disable IDE1006

using Microsoft.VisualStudio.CommandBars;
using SQLAid.Extensions;
using stdole;
using System.Drawing;
using IconConverter = SQLAid.Helpers.IconConverter;

namespace SQLAid.Integration.DTE.SqlControl
{
    internal static class CommandBarControlBuilder
    {
        public static CommandBarControl Caption(this CommandBarControl commandBarControl, string caption)
        {
            commandBarControl.Caption = caption;
            return commandBarControl;
        }

        public static CommandBarControl Visible(this CommandBarControl commandBarControl, bool visible)
        {
            commandBarControl.Visible = commandBarControl.Enabled = visible;
            return commandBarControl;
        }

        public static CommandBarControl TooltipText(this CommandBarControl commandBarControl, string tooltipText)
        {
            commandBarControl.TooltipText = tooltipText;
            return commandBarControl;
        }
    }

    internal static class CommandBarButtonBuilder
    {
        public static CommandBarButton AddIcon(this CommandBarButton commandBarButton, string filename)
        {
            commandBarButton.Style = MsoButtonStyle.msoButtonIconAndCaption;
            return AddIcon(commandBarButton, new Icon(filename));
        }

        public static CommandBarButton AddIcon(this CommandBarButton commandBarButton, Icon icon)
        {
            commandBarButton.Picture = IconConverter.GetPictureDispFromImage(icon.ToBitmap()).As<StdPicture>();
            return commandBarButton;
        }
    }
}