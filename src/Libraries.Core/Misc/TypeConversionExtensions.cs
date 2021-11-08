using System;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Error.Logic;

namespace Nexus.Link.Libraries.Core.Misc
{
    /// <summary>
    /// Help methods for mapping
    /// </summary>
    public static class TypeConversionExtensions
    {
        public static Guid? ToNullableGuid(this string source)
        {
            if (source == null) return null;
            return source.ToGuid();
        }
        public static Guid ToGuid(this string source)
        {
            InternalContract.RequireNotNull(source, nameof(source));
            var success = Guid.TryParse(source, out var valueAsGuid);
            InternalContract.Require(success, $"Could not parse parameter {nameof(source)} ({source}) of type {nameof(String)} into type {nameof(Guid)}.");
            return valueAsGuid;
        }

        public static string ToLowerCaseString(this Guid source)
        {
            return source.ToString().ToLowerInvariant();
        }

        public static string ToLowerCaseString(this Guid? source)
        {
            if (source == null) return null;
            return source.ToString().ToLowerInvariant();
        }

        public static int ToInt(this string source)
        {
            InternalContract.RequireNotNull(source, nameof(source));
            var success = int.TryParse(source, out var valueAsInt);
            InternalContract.Require(success, $"Could not parse parameter {nameof(source)} ({source}) of type {nameof(String)} into type {nameof(Int32)}.");
            return valueAsInt;
        }

        public static T ToEnum<T>(this string source) where T : struct
        {
            InternalContract.RequireNotNull(source, nameof(source));
            InternalContract.Require(typeof(T).IsEnum, $"The generic type {typeof(T).Name} must be an enum.");
            var success = Enum.TryParse<T>(source, out var valueAsEnum);
            InternalContract.Require(success, $"Could not parse parameter {nameof(source)} ({source}) of type {nameof(String)} into type {nameof(T)}.");
            return valueAsEnum;
        }

        /// <summary>
        /// Map an id between two types.
        /// </summary>
        /// <param name="source">The id to map.</param>
        /// <typeparam name="TTarget">The target type.</typeparam>
        /// <typeparam name="TSource">The source type.</typeparam>
        /// <exception cref="FulcrumNotImplementedException">Thrown if the type was not recognized. Please add that type to the class <see cref="TypeConversionExtensions"/>.</exception>
        public static TTarget MapToType<TTarget, TSource>(TSource source)
        {
            var sourceType = typeof(TSource);
            var targetType = typeof(TTarget);
            if (targetType == typeof(string))
            {
                if (source == null) return (TTarget) (object) null;
                var result = source.ToString();
                if (sourceType == typeof(Guid) || sourceType == typeof(Guid?))
                {
                    result = result.ToLowerInvariant();
                }
                return (TTarget)(object)result;
            }
            InternalContract.Require(!targetType.IsEnum, $"Use MapToStruct for mapping of enums.");
            if (targetType == typeof(Guid) || targetType == typeof(Guid?))
            {
                if (targetType == typeof(Guid))
                {
                    InternalContract.RequireNotNull(source, nameof(source));
                }
                if (source == null) return (TTarget) (object) null;
                return (TTarget)(object) source.ToString().ToGuid();
            }
            if (targetType == typeof(int) || targetType == typeof(int?))
            {
                if (targetType == typeof(int))
                {
                    InternalContract.RequireNotNull(source, nameof(source));
                }
                if (source == null) return (TTarget) (object) null;
                return (TTarget)(object) source.ToString().ToInt();
            }
            throw new FulcrumNotImplementedException($"There is currently no rule on how to convert an id from type {sourceType.Name} to type {targetType.Name}.");
        }

        /// <summary>
        /// Map an id between two types.
        /// </summary>
        /// <param name="source">The id to map.</param>
        /// <typeparam name="TTarget">The target type.</typeparam>
        /// <typeparam name="TSource">The source type.</typeparam>
        /// <exception cref="FulcrumNotImplementedException">Thrown if the type was not recognized. Please add that type to the class <see cref="TypeConversionExtensions"/>.</exception>
        public static TTarget? MapToStructOrNull<TTarget, TSource>(TSource source) where TTarget : struct
        {
            if (source == null) return null;
            return MapToStruct<TTarget, TSource>(source);
        }

        /// <summary>
        /// Map an id between two types.
        /// </summary>
        /// <param name="source">The id to map.</param>
        /// <typeparam name="TTarget">The target type.</typeparam>
        /// <typeparam name="TSource">The source type.</typeparam>
        /// <exception cref="FulcrumNotImplementedException">Thrown if the type was not recognized. Please add that type to the class <see cref="TypeConversionExtensions"/>.</exception>
        public static TTarget MapToStruct<TTarget, TSource>(TSource source) where TTarget : struct
        {
            InternalContract.RequireNotNull(source, nameof(source));
            var sourceType = typeof(TSource);
            var targetType = typeof(TTarget);
            if (targetType.IsEnum)
            {
                if (sourceType == typeof(string))
                {
                    return source.ToString().ToEnum<TTarget>();
                }
            }
            throw new FulcrumNotImplementedException($"There is currently no rule on how to convert an id from type {sourceType.Name} to type {targetType.Name}.");
        }
    }
}
