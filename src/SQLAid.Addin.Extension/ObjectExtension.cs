using System;
using System.ComponentModel;
using System.Reflection;

namespace SQLAid.Addin.Extension
{
    public static class ObjectExtension
    {
        public static T As<T>(this object @value)
        {
            return (T)@value.As(typeof(T));
        }

        public static object As(this object @value, Type conversionType)
        {
            if (value == null)
                return default;

            if (@value is string _value)
            {
                if (conversionType == typeof(string))
                    return _value;

                if (conversionType == typeof(Guid))
                    return new Guid(Convert.ToString(_value));

                if (string.IsNullOrWhiteSpace(_value) && conversionType != typeof(string))
                    return default;

                if (conversionType.IsEnum)
                {
                    if (Enum.IsDefined(conversionType, _value))
                        return Enum.Parse(conversionType, _value, true);

                    throw new InvalidOperationException("Invalid Cast Enum");
                }
            }
            else
            {
                if (conversionType == typeof(string))
                    return Convert.ToString(@value);
            }

            if (conversionType.IsGenericType && conversionType.GetGenericTypeDefinition().Equals(typeof(Nullable<>)))
                conversionType = new NullableConverter(conversionType).UnderlyingType;

            if (conversionType.IsEnum && @value.GetType().IsValueType && !@value.GetType().IsEnum)
            {
                if (Enum.IsDefined(conversionType, @value))
                    return Enum.ToObject(conversionType, @value);

                throw new InvalidOperationException("Invalid Cast Enum");
            }

            if (conversionType is IConvertible || conversionType.IsValueType && !conversionType.IsEnum)
                return Convert.ChangeType(@value, conversionType);

            return value;
        }

        public static object GetNonPublicField(this object @object, string propertyName)
        {
            var field = @object.GetType()
                .GetField(propertyName, BindingFlags.NonPublic | BindingFlags.Instance);
            return field.GetValue(@object);
        }

        public static object GetPublicField(object @object, string field)
        {
            var f = @object.GetType().GetField(field, BindingFlags.Public | BindingFlags.Instance);
            return f.GetValue(@object);
        }

        public static object GetPublicProperty(this object @object, string propertyName)
        {
            var p = @object.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.GetProperty);
            return p.GetValue(@object, null);
        }

        public static object GetNonPublicProperty(this object @object, string propertyName)
        {
            var p = @object.GetType().GetProperty(propertyName, BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.GetProperty);
            return p.GetValue(@object, null);
        }
    }
}