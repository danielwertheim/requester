using System;
using System.Collections;
using System.Reflection;

namespace Requester.Validation
{
    public static class Should
    {
        public static void ShouldBeValueEqualTo<T>(this T x, T y)
        {
            AreValueEqual(typeof(T), x, y);
        }

        private static void AreValueEqual(Type type, object a, object b)
        {
            if (ReferenceEquals(a, b))
                return;

            if (a == null && b == null)
                return;

            if (a == null || b == null)
                throw AssertionExceptionFactory.Create("Values of type {0} was found to be different.", type.Name);

            if (type.IsEnumerableType())
            {
                var enum1 = a as IEnumerable;
                if (enum1 == null)
                    throw AssertionExceptionFactory.Create("Value of type {0} was found to be null. Expected an enumerable instance.");
                
                var enum2 = b as IEnumerable;
                if(enum2 == null)
                    throw AssertionExceptionFactory.Create("Value of type {0} was found to be null. Expected an enumerable instance.");

                var e1 = enum1.GetEnumerator();
                var e2 = enum2.GetEnumerator();

                while (e1.MoveNext() && e2.MoveNext())
                {
                    AreValueEqual(e1.Current.GetType(), e1.Current, e2.Current);
                }
                return;
            }

            if (type == typeof(object))
                throw AssertionExceptionFactory.Create("Equality comparision not feasible to type {0}.", type.Name);

            if (type.IsSimpleType())
            {
                if (!Equals(a, b))
                    throw AssertionExceptionFactory.Create("Comparing simple types of '{0}', expected them to be equal but '{1}' != '{2}'", type.Name, a, b);

                return;
            }

            var properties = type.GetProperties();
            if (properties.Length == 0)
            {
                if (!Equals(a, b))
                    throw AssertionExceptionFactory.Create("Comparing simple types of '{0}', expected them to be equal but '{1}' != '{2}'", type.Name, a, b);
            }

            foreach (var propertyInfo in type.GetProperties())
            {
                var propertyType = propertyInfo.PropertyType;
                var valueForA = propertyInfo.GetValue(a, null);
                var valueForB = propertyInfo.GetValue(b, null);

                var isSimpleType = propertyType.IsSimpleType();
                if (isSimpleType)
                {
                    if (!Equals(valueForA, valueForB))
                        throw AssertionExceptionFactory.Create("Values in property '{0}' doesn't match.", propertyInfo.Name);
                }
                else
                    AreValueEqual(propertyType, valueForA, valueForB);
            }
        } 
    }
}