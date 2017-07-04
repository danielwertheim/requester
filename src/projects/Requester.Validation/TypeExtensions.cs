using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace Requester.Validation
{
    internal static class TypeExtensions
    {
        private static readonly Type EnumerableType = typeof(IEnumerable);
        private static readonly Type StringType = typeof(string);
        private static readonly Type NullableType = typeof(Nullable<>);
        private static readonly HashSet<Type> ExtraPrimitiveTypes = new HashSet<Type> { typeof(string), typeof(Guid), typeof(DateTime), typeof(decimal) };
        private static readonly HashSet<Type> ExtraPrimitiveNullableTypes = new HashSet<Type> { typeof(Guid?), typeof(DateTime?), typeof(decimal?) };

        internal static bool IsSimpleType(this Type type)
        {
            var info = type.GetTypeInfo();

            return (info.IsGenericType == false && info.IsValueType) || info.IsPrimitive || info.IsEnum || ExtraPrimitiveTypes.Contains(type) || type.IsNullablePrimitiveType();
        }

        internal static bool IsEnumerableType(this Type type)
        {
            if (type == StringType)
                return false;

            var info = type.GetTypeInfo();

            return info.IsValueType == false
                   && info.IsPrimitive == false
                   && EnumerableType.IsAssignableFrom(type);
        }

        private static bool IsNullablePrimitiveType(this Type t)
        {
            if (ExtraPrimitiveNullableTypes.Contains(t))
                return true;

            var info = t.GetTypeInfo();
            if (!info.IsValueType)
                return false;

            return info.IsGenericType && t.GetGenericTypeDefinition() == NullableType && t.GetGenericArguments()[0].GetTypeInfo().IsPrimitive;
        }
    }
}