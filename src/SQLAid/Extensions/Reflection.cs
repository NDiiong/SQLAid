using System;
using System.Linq;
using System.Reflection;

namespace SQLAid.Extensions
{
    public static class Reflection
    {
        public static object GetField(object target, string fieldName)
        {
            var field = target.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);

            if (field == null)
                throw new ArgumentOutOfRangeException($"Missing field {fieldName} on type {target.GetType()}");

            return field.GetValue(target);
        }

        public static object GetBaseTypeField(object target, string fieldName)
        {
            var field = target.GetType().BaseType.GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.IgnoreCase | BindingFlags.Instance);

            if (field == null)
                throw new ArgumentOutOfRangeException($"Missing field {fieldName} on type {target.GetType()}");

            return field.GetValue(target);
        }

        public static void SetField(object target, string fieldName, object value)
        {
            var field = target.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);

            if (field == null)
                throw new ArgumentOutOfRangeException($"Missing field {fieldName} on type {target.GetType()}");

            field.SetValue(target, value);
        }

        public static object GetPropertyValue(object target, string propName)
        {
            return target.GetType().GetProperty(propName, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance).GetValue(target);
        }

        public static object GetProperty(object target, string propName)
        {
            var prop = target.GetType().GetProperty(propName, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);

            if (prop == null)
                throw new ArgumentOutOfRangeException($"Missing property {propName} on type {target.GetType()}");

            return prop.GetValue(target);
        }

        public static void SetProperty(object target, string propName, object value)
        {
            var prop = target.GetType().GetProperty(propName, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);

            if (prop == null)
                throw new ArgumentOutOfRangeException($"Missing property {propName} on type {target.GetType()}");

            prop.SetValue(target, value);
        }

        public static object InvokeMethod(object target, string methodName, params object[] args)
        {
            const BindingFlags bindingFlags = BindingFlags.InvokeMethod | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance;

            var type = target.GetType();
            while (type != null)
            {
                var methods = type.GetMethods(bindingFlags);
                var method = methods
                    .Where(m => m.Name == methodName && m.GetParameters().Length == args.Length)
                    .ToArray();

                if (method.Length == 1)
                    return method[0].Invoke(target, args);

                if (method.Length > 1)
                    throw new ArgumentOutOfRangeException($"Ambiguous method {methodName} on type {target.GetType()}");

                type = type.BaseType;
            }

            throw new ArgumentOutOfRangeException($"Missing method {methodName} on type {target.GetType()}");
        }

        public static T InvokeMethod<T>(object target, string methodName, params object[] args)
        {
            return target.InvokeMethod(methodName, args).As<T>();
        }
    }
}