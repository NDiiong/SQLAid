using Microsoft.CSharp;
using System;
using System.CodeDom;

namespace SQLAid.Helpers
{
    public static class TypeHelper
    {
        public static string Primitive(Type type)
        {
            using (var provider = new CSharpCodeProvider())
            {
                if (type.IsGenericType && type.GetGenericTypeDefinition().Equals(typeof(Nullable<>)))
                    return provider.GetTypeOutput(new CodeTypeReference(type.GetGenericArguments()[0])).Replace("System.", "") + "?";

                return provider.GetTypeOutput(new CodeTypeReference(type)).Replace("System.", "");
            }
        }
    }
}