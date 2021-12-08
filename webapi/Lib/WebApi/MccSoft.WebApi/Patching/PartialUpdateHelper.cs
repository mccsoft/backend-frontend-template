using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using MccSoft.LowLevelPrimitives.Exceptions;
using MccSoft.WebApi.Patching.Models;

namespace MccSoft.WebApi.Patching
{
    /// <summary>
    /// Class is used in most HttpPatch requests for updating the DB entity
    /// with properties that are present in BODY of request.
    /// Properties that are not passed preserve their previous values.
    /// </summary>
    public static class PartialUpdateHelper
    {
        /// <summary>
        /// Used in most HttpPatch requests for updating the DB entity
        /// with properties that are present in BODY of request.
        /// Properties that are not passed preserve their previous values.
        /// </summary>
        public static void Update<TObjectToUpdate, TObjectWithNewValues>(
            this TObjectToUpdate objectToUpdate,
            TObjectWithNewValues objectWithNewValues
        ) where TObjectWithNewValues : IPatchRequest
        {
            var objectWithNewValuesProperties = objectWithNewValues.GetType()
                .GetProperties()
                .Where(x => x.GetGetMethod() != null)
                .ToList();
            var typeToUpdate = typeof(TObjectToUpdate);

            foreach (var propertyInNewValuesObject in objectWithNewValuesProperties)
            {
                var propertyName = propertyInNewValuesObject.Name;
                var propertyInObjectToUpdate = typeToUpdate.GetProperty(propertyName);
                if (
                    propertyInObjectToUpdate != null
                    && (
                        objectWithNewValues.IsFieldPresent(propertyName)
                        || propertyInObjectToUpdate.PropertyType.IsIList()
                    )
                    && propertyInObjectToUpdate.SetMethod != null
                    && propertyInObjectToUpdate.SetMethod.IsPublic
                ) {
                    var propertyValue = propertyInNewValuesObject.GetValue(objectWithNewValues);
                    Type propertyType =
                        Nullable.GetUnderlyingType(propertyInObjectToUpdate.PropertyType)
                        ?? propertyInObjectToUpdate.PropertyType;
                    if (propertyType.IsEnum)
                    {
                        if (!Enum.IsDefined(propertyType, Convert.ToInt32(propertyValue)))
                        {
                            throw new ValidationException(
                                propertyName,
                                $"Value '{propertyValue}' is out of range"
                            );
                        }
                    }

                    if (
                        propertyInNewValuesObject.PropertyType.GetInterfaces()
                            .Contains(typeof(IPatchRequest))
                    ) {
                        var newValue =
                            propertyInObjectToUpdate.GetValue(objectToUpdate)
                            ?? Activator.CreateInstance(propertyType);

                        try
                        {
                            typeof(PartialUpdateHelper).GetMethod("Update")
                                .MakeGenericMethod(
                                    propertyInObjectToUpdate.PropertyType,
                                    propertyInNewValuesObject.PropertyType
                                )
                                .Invoke(null, new[] { newValue, propertyValue });
                        }
                        catch (TargetInvocationException e)
                        {
                            throw e.InnerException;
                        }

                        propertyInObjectToUpdate.SetValue(objectToUpdate, newValue);
                    }
                    else if (propertyType.IsIList())
                    {
                        if (propertyValue is not IEnumerable enumerable)
                            continue;

                        if (propertyType.HasDefaultConstructor())
                        {
                            var newEnumerable = Activator.CreateInstance(propertyType);
                            // ReSharper disable once PossibleNullReferenceException
                            // enumerable is IList
                            foreach (var o in enumerable)
                                if (o.GetType().HasDefaultConstructor())
                                {
                                    var newObject = Activator.CreateInstance(
                                        propertyType.GetTFromListT()!
                                    );
                                    BlindMap(newObject, o);
                                    // ReSharper disable once PossibleNullReferenceException
                                    // newEnumerable is IList
                                    (newEnumerable as IList).Add(newObject);
                                }
                                else
                                {
                                    // ReSharper disable once PossibleNullReferenceException
                                    // newEnumerable is IList
                                    (newEnumerable as IList).Add(o);
                                }

                            propertyInObjectToUpdate.SetValue(objectToUpdate, newEnumerable);
                        }
                        else
                        {
                            throw new InvalidOperationException(
                                "Arrays are not supported for mapping"
                            );
                        }
                    }
                    else
                    {
                        propertyInObjectToUpdate.SetValue(objectToUpdate, propertyValue);
                    }
                }
            }
        }

        public static TResult GetValue<T, TResult>(
            this PatchRequest<T> patchRequest,
            Expression<Func<T, TResult>> field
        ) {
            var propertyName = ((System.Linq.Expressions.MemberExpression)field.Body).Member.Name;
            return (TResult)patchRequest.GetType().GetProperty(propertyName).GetValue(patchRequest);
        }

        private static bool HasDefaultConstructor(this Type t)
        {
            return t.IsValueType || t.GetConstructor(Type.EmptyTypes) != null;
        }

        private static bool IsIList(this Type type)
        {
            return type.GetInterfaces()
                .Append(type)
                .Any(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IList<>));
        }

        private static Type? GetTFromListT(this Type type)
        {
            return type.GetInterfaces()
                .Append(type)
                .FirstOrDefault(
                    x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IList<>)
                )?.GetGenericArguments().FirstOrDefault();
        }

        private static void BlindMap<TObjectToUpdate, TObjectWithNewValues>(
            this TObjectToUpdate objectToUpdate,
            TObjectWithNewValues objectWithNewValues
        ) {
            var objectWithNewValuesProperties = objectWithNewValues.GetType()
                .GetProperties()
                .Where(x => x.GetGetMethod() != null)
                .ToList();
            var typeToUpdate = objectToUpdate.GetType();

            foreach (var propertyInNewValuesObject in objectWithNewValuesProperties)
            {
                var propertyName = propertyInNewValuesObject.Name;
                var propertyInObjectToUpdate = typeToUpdate.GetProperty(propertyName);
                if (
                    propertyInObjectToUpdate != null
                    && propertyInObjectToUpdate.SetMethod != null
                    && propertyInObjectToUpdate.SetMethod.IsPublic
                ) {
                    var propertyValue = propertyInNewValuesObject.GetValue(objectWithNewValues);
                    Type propertyType =
                        Nullable.GetUnderlyingType(propertyInObjectToUpdate.PropertyType)
                        ?? propertyInObjectToUpdate.PropertyType;
                    if (propertyType.IsEnum)
                    {
                        if (!Enum.IsDefined(propertyType, Convert.ToInt32(propertyValue)))
                        {
                            throw new ValidationException(
                                propertyName,
                                $"Value '{propertyValue}' is out of range"
                            );
                        }
                    }
                    else
                    {
                        propertyInObjectToUpdate.SetValue(objectToUpdate, propertyValue);
                    }
                }
            }
        }
    }
}
