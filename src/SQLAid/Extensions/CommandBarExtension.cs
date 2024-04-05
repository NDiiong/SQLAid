#pragma warning disable IDE1006

using Microsoft.VisualStudio.CommandBars;
using SQLAid.Integration.DTE.Commandbars;
using System;

namespace SQLAid.Extensions
{
    public static class SqlTypesExtensions
    {
        public static Type SqlToType(this string pSqlType)
        {
            switch (pSqlType)
            {
                case "bigint":
                case "real":
                    return typeof(long);

                case "numeric":
                    return typeof(decimal);

                case "bit":
                    return typeof(bool);

                case "smallint":
                    return typeof(short);

                case "decimal":
                case "smallmoney":
                case "money":
                    return typeof(decimal);

                case "int":
                    return typeof(int);

                case "tinyint":
                    return typeof(byte);

                case "float":
                    return typeof(float);

                case "date":
                case "datetime2":
                case "smalldatetime":
                case "datetime":
                case "time":
                    return typeof(DateTime);

                case "datetimeoffset":
                    return typeof(DateTimeOffset);

                case "char":
                case "varchar":
                case "text":
                case "nchar":
                case "nvarchar":
                case "ntext":
                    return typeof(string);

                case "binary":
                case "varbinary":
                case "image":
                    return typeof(byte[]);

                case "uniqueidentifier":
                    return typeof(Guid);

                default:
                    return typeof(string);
            }
        }
    }

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