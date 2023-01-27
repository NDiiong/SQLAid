using System;
using System.ComponentModel;

namespace SQLAid.Addin.Extension
{
    public static class ObjectExtension
    {
        private static object As(this object @value, Type conversionType)
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

        public static T As<T>(this object @value)
        {
            return (T)@value.As(typeof(T));
        }

        public static object GetField(this object @object, string propName)
        {
            return Reflection.GetField(@object, propName);
        }

        public static T GetField<T>(this object @object, string propName)
        {
            return Reflection.GetField(@object, propName).As<T>();
        }

        public static T GetBaseTypeField<T>(this object @object, string propName)
        {
            return Reflection.GetBaseTypeField(@object, propName).As<T>();
        }

        public static object GetProperty(this object @object, string propName)
        {
            return Reflection.GetProperty(@object, propName);
        }

        public static T GetProperty<T>(this object @object, string propName)
        {
            return Reflection.GetProperty(@object, propName).As<T>();
        }

        public static object InvokeMethod(this object @object, string methodName, params object[] args)
        {
            return Reflection.InvokeMethod(@object, methodName, args);
        }

        public static T InvokeMethod<T>(this object @object, string methodName, params object[] args)
        {
            return Reflection.InvokeMethod(@object, methodName, args).As<T>();
        }
    }
}