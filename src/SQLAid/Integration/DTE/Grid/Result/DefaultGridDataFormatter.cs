using System;
using System.Collections.Generic;
using System.Globalization;

namespace SQLAid.Integration.DTE.Grid.Result
{
    public interface IGridDataFormatter : IDisposable
    {
        object FormatCellValue(string value, Type columnType);
    }

    public sealed class DefaultGridDataFormatter : IGridDataFormatter
    {
        private static readonly HashSet<Type> _numericTypes = new HashSet<Type>
        {
            typeof(int), typeof(decimal), typeof(long),
            typeof(double), typeof(float), typeof(byte)
        };

        private static readonly HashSet<Type> _specialTypes = new HashSet<Type>
        {
            typeof(Guid), typeof(DateTime),
            typeof(DateTimeOffset), typeof(byte[])
        };

        public object FormatCellValue(string value, Type columnType)
        {
            if (string.IsNullOrEmpty(value) || value.Equals("NULL", StringComparison.OrdinalIgnoreCase))
                return value;

            if (columnType == typeof(bool))
                return value == "1" ? 1 : 0;

            if (_numericTypes.Contains(columnType))
                return Convert.ChangeType(value, columnType, CultureInfo.InvariantCulture);

            if (_specialTypes.Contains(columnType))
                return FormatSpecialType(value);

            return $"N'{value.Replace("'", "''")}'";
        }

        private string FormatSpecialType(string value)
        {
            var converted = Convert.ChangeType(value, typeof(string), CultureInfo.InvariantCulture);
            return $"N'{converted}'";
        }

        public void Dispose()
        { }
    }
}