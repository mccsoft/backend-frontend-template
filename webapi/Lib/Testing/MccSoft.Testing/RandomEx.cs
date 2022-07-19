using Castle.DynamicProxy.Internal;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace MccSoft.Testing
{
    /// <summary>
    /// Helpers to generate random values.
    /// </summary>
    public static class RandomEx
    {
        /// <summary>
        /// The hard-coded seed that is used for consistency of test runs.
        /// </summary>
        private const int _seed = 1;

        private static readonly Random _rnd = new Random(_seed);

        /// <summary>
        /// Creates a copy of the specified list, where elements are shuffled in a random order.
        /// </summary>
        /// <remarks>
        /// See https://en.wikipedia.org/wiki/Fisher%E2%80%93Yates_shuffle
        /// </remarks>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="original">The original list.</param>
        /// <returns>The shuffled list.</returns>
        public static List<T> Shuffle<T>(List<T> original)
        {
            lock (_rnd)
            {
                List<T> list = original.ToList();
                for (int i = 0; i < original.Count; i++)
                {
                    int j = _rnd.Next(i);
                    T t = list[i];
                    list[i] = list[j];
                    list[j] = t;
                }

                return list;
            }
        }

        /// <summary>
        /// Assigns random values to properties of types double?, int, DateTime, Enum, IList
        /// or of complex reference type of the specified object.
        /// </summary>
        /// <param name="obj">The object to initialize.</param>
        public static void RandomizeProperties(object obj)
        {
            if (obj == null)
            {
                throw new ArgumentNullException(nameof(obj));
            }

            PropertyInfo[] propertyInfos = obj.GetType()
                .GetProperties()
                .Where(p => p.CanWrite)
                .ToArray();

            var doublePropertyInfos = propertyInfos.Where(
                p => p.PropertyType == typeof(double?) || p.PropertyType == typeof(double)
            );
            var intPropertyInfos = propertyInfos.Where(
                p => p.PropertyType == typeof(int) || p.PropertyType == typeof(int?)
            );
            var dateTimePropertyInfos = propertyInfos.Where(
                p => p.PropertyType == typeof(DateTime) || p.PropertyType == typeof(DateTime?)
            );

            var timeSpanPropertyInfos = propertyInfos.Where(
                p => p.PropertyType == typeof(TimeSpan)
            );

            var objectPropertyInfos = propertyInfos.Where(
                p =>
                    !p.PropertyType.IsValueType
                    && p.PropertyType != typeof(string)
                    && !typeof(ICollection).IsAssignableFrom(p.PropertyType)
            );
            var stringPropertyInfos = propertyInfos.Where(p => p.PropertyType == typeof(string));
            var enumPropertyInfos = propertyInfos.Where(p =>
            {
                Type propertyType = Nullable.GetUnderlyingType(p.PropertyType) ?? p.PropertyType;

                return typeof(Enum).IsAssignableFrom(propertyType);
            });
            var listPropertyInfos = propertyInfos.Where(
                p => typeof(IList).IsAssignableFrom(p.PropertyType)
            );
            var boolPropertyInfos = propertyInfos.Where(p =>
            {
                Type propertyType = Nullable.GetUnderlyingType(p.PropertyType) ?? p.PropertyType;

                return typeof(Boolean).IsAssignableFrom(propertyType);
            });

            ProcessDoubles(obj, doublePropertyInfos);
            ProcessIntegers(obj, intPropertyInfos);
            ProcessDateTimes(obj, dateTimePropertyInfos);
            ProcessTimeSpans(obj, timeSpanPropertyInfos);
            ProcessObjects(obj, objectPropertyInfos);
            ProcessStrings(obj, stringPropertyInfos);
            ProcessEnums(obj, enumPropertyInfos);
            ProcessLists(obj, listPropertyInfos);
            ProcessBool(obj, boolPropertyInfos);
        }

        private static void ProcessStrings(object obj, IEnumerable<PropertyInfo> propertyInfos)
        {
            foreach (PropertyInfo propertyInfo in propertyInfos)
            {
                string value = GetRandomStringValue();

                propertyInfo.SetValue(obj, value);
            }
        }

        private static string GetRandomStringValue()
        {
            string value;
            lock (_rnd)
            {
                value = _rnd.Next().ToString();
            }

            return value;
        }

        private static void ProcessObjects(object obj, IEnumerable<PropertyInfo> propertyInfos)
        {
            foreach (PropertyInfo propertyInfo in propertyInfos)
            {
                ProcessObject(obj, propertyInfo);
            }
        }

        private static void ProcessLists(object obj, IEnumerable<PropertyInfo> propertyInfos)
        {
            foreach (PropertyInfo propertyInfo in propertyInfos)
            {
                var type = propertyInfo.PropertyType;
                var instance = Activator.CreateInstance(type);

                if (type.IsGenericType)
                {
                    IList list = (IList)instance;
                    Type genericType = type.GetGenericArguments()[0];
                    object item = null;
                    if (typeof(Enum).IsAssignableFrom(genericType))
                    {
                        item = GetRandomEnumValue(genericType);
                    }
                    else if (genericType == typeof(double?))
                    {
                        item = GetRandomDoubleValue();
                    }
                    else if (genericType == typeof(int) || genericType == typeof(int?))
                    {
                        item = GetRandomIntValue();
                    }
                    else if (genericType == typeof(string))
                    {
                        item = GetRandomStringValue();
                    }
                    else if (!genericType.IsValueType)
                    {
                        item = Activator.CreateInstance(genericType);
                        RandomizeProperties(item);
                    }

                    if (item != null)
                    {
                        list.Add(item);
                    }
                }

                propertyInfo.SetValue(obj, instance);
            }
        }

        private static void ProcessObject(object obj, PropertyInfo propertyInfo)
        {
            var type = propertyInfo.PropertyType;
            var instance = Activator.CreateInstance(type);

            RandomizeProperties(instance);

            propertyInfo.SetValue(obj, instance);
        }

        private static void ProcessDateTimes(object obj, IEnumerable<PropertyInfo> propertyInfos)
        {
            foreach (PropertyInfo propertyInfo in propertyInfos)
            {
                DateTime value;
                lock (_rnd)
                {
                    value = DateTime.FromOADate(_rnd.NextDouble() * 1000);
                }

                propertyInfo.SetValue(obj, value);
            }
        }

        private static void ProcessTimeSpans(object obj, IEnumerable<PropertyInfo> propertyInfos)
        {
            foreach (PropertyInfo propertyInfo in propertyInfos)
            {
                TimeSpan value;
                lock (_rnd)
                {
                    value = TimeSpan.FromSeconds(_rnd.NextDouble() * 84600); // 1 day max
                }

                propertyInfo.SetValue(obj, value);
            }
        }

        private static void ProcessIntegers(object obj, IEnumerable<PropertyInfo> propertyInfos)
        {
            foreach (PropertyInfo propertyInfo in propertyInfos)
            {
                int value = GetRandomIntValue();

                propertyInfo.SetValue(obj, value);
            }
        }

        private static int GetRandomIntValue()
        {
            int value;
            lock (_rnd)
            {
                value = _rnd.Next();
            }

            return value;
        }

        private static void ProcessDoubles(object obj, IEnumerable<PropertyInfo> propertyInfos)
        {
            foreach (PropertyInfo propertyInfo in propertyInfos)
            {
                double? value = GetRandomDoubleValue();

                propertyInfo.SetValue(obj, value);
            }
        }

        private static double? GetRandomDoubleValue()
        {
            double? value;
            lock (_rnd)
            {
                value = _rnd.NextDouble();
            }

            return value;
        }

        private static void ProcessEnums(object obj, IEnumerable<PropertyInfo> propertyInfos)
        {
            foreach (PropertyInfo propertyInfo in propertyInfos)
            {
                // Set always the first possible enum value
                object enumValue = GetRandomEnumValue(propertyInfo.PropertyType);

                propertyInfo.SetValue(obj, enumValue);
            }
        }

        private static void ProcessBool(object obj, IEnumerable<PropertyInfo> propertyInfos)
        {
            foreach (PropertyInfo propertyInfo in propertyInfos)
            {
                // Set it always to value true
                propertyInfo.SetValue(obj, true);
            }
        }

        private static object GetRandomEnumValue(Type enumType)
        {
            enumType = Nullable.GetUnderlyingType(enumType) ?? enumType;
            Array enumValues = Enum.GetValues(enumType);
            IEnumerator enumerator = enumValues.GetEnumerator();
            enumerator.MoveNext();
            object enumValue = enumerator.Current;
            return enumValue;
        }
    }
}
