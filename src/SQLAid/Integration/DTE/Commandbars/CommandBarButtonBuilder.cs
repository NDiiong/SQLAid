#pragma warning disable IDE1006

using Microsoft.VisualStudio.CommandBars;
using SQLAid.Extensions;
using stdole;
using System.Drawing;
using IconConverter = SQLAid.Helpers.IconConverter;

namespace SQLAid.Integration.DTE.Commandbars
{
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

        public static CommandBarButton AddStyle(this CommandBarButton commandBarButton, MsoButtonStyle style)
        {
            commandBarButton.Style = style;
            return commandBarButton;
        }
    }
}