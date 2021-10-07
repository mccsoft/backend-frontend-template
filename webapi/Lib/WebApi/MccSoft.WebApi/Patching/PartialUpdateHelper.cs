using System;
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
                .Where(x => x.GetCustomAttribute<DoNotPatchAttribute>() == null)
                .ToList();
            var typeToUpdate = typeof(TObjectToUpdate);

            foreach (var propertyInNewValuesObject in objectWithNewValuesProperties)
            {
                var propertyName = propertyInNewValuesObject.Name;
                var propertyInObjectToUpdate = typeToUpdate.GetProperty(propertyName);
                if (
                    propertyInObjectToUpdate != null
                    && objectWithNewValues.IsFieldPresent(propertyName)
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

                    propertyInObjectToUpdate.SetValue(objectToUpdate, propertyValue);
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
    }
}
