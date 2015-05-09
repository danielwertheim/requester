using System;
using System.Collections;
using System.Collections.Generic;

namespace Requester.Validation
{
    internal static class TypeExtensions
    {
        private static readonly Type EnumerableType = typeof(IEnumerable);
        private static readonly Type StringType = typeof(string);
        private static readonly Type NullableType = typeof(Nullable<>);
        private static readonly HashSet<Type> ExtraPrimitiveTypes = new HashSet<Type> { typeof(string), typeof(Guid), typeof(DateTime), typeof(Decimal) };
        private static readonly HashSet<Type> ExtraPrimitiveNullableTypes = new HashSet<Type> { typeof(Guid?), typeof(DateTime?), typeof(Decimal?) };

        internal static bool IsSimpleType(this Type type)
        {
            return (type.IsGenericType == false && type.IsValueType) || type.IsPrimitive || type.IsEnum || ExtraPrimitiveTypes.Contains(type) || type.IsNullablePrimitiveType();
        }

        internal static bool IsEnumerableType(this Type type)
        {
            return type != StringType
                   && type.IsValueType == false
                   && type.IsPrimitive == false
                   && EnumerableType.IsAssignableFrom(type);
        }

        private static bool IsNullablePrimitiveType(this Type t)
        {
            return ExtraPrimitiveNullableTypes.Contains(t) || (t.IsValueType && t.IsGenericType && t.GetGenericTypeDefinition() == NullableType && t.GetGenericArguments()[0].IsPrimitive);
        }
    }
}