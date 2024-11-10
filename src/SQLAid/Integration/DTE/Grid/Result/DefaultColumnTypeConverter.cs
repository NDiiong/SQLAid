using System;
using System.Globalization;
using System.Text;

namespace SQLAid.Integration.DTE.Grid.Result
{
    public interface IColumnTypeConverter : IDisposable
    {
        object ConvertToType(string cellText, Type dataType);
    }

    public class DefaultColumnTypeConverter : IColumnTypeConverter
    {
        public object ConvertToType(string value, Type targetType)
        {
            if (string.IsNullOrEmpty(value) || value.Equals("NULL", StringComparison.OrdinalIgnoreCase))
                return null;

            try
            {
                if (targetType == typeof(bool))
                    return value != "0";

                if (targetType == typeof(Guid))
                    return new Guid(value);

                if (targetType == typeof(DateTime) || targetType == typeof(DateTimeOffset))
                    return DateTime.Parse(value, CultureInfo.InvariantCulture);

                if (targetType == typeof(byte[]))
                    return Encoding.UTF8.GetBytes(value);

                return Convert.ChangeType(value, targetType, CultureInfo.InvariantCulture);
            }
            catch (Exception ex)
            {
                throw new TypeConversionException($"Failed to convert '{value}' to type {targetType.Name}", ex);
            }
        }

        public void Dispose()
        { }
    }
}