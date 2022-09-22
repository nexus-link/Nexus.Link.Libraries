using System;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Crud.Model;
using Nexus.Link.Libraries.Core.Error.Logic;
using Nexus.Link.Libraries.Crud.Model;

namespace Nexus.Link.Libraries.Crud.Helpers
{
    /// <summary>
    /// Help methods for mapping
    /// </summary>
    public static class MapperHelper
    {

        /// <summary>
        /// Map a lock between two types.
        /// </summary>
        /// <param name="source">The lock to map.</param>
        /// <exception cref="FulcrumNotImplementedException">Thrown if the type was not recognized. Please add that type to the class <see cref="MapperHelper"/>.</exception>

        public static Lock<TTarget> MapToType<TTarget, TSource>(Lock<TSource> source)
        {
            return new Lock<TTarget>
            {
                LockId = Core.Misc.TypeConversionExtensions.MapToType<TTarget, TSource>(source.LockId),
                ItemId = Core.Misc.TypeConversionExtensions.MapToType<TTarget, TSource>(source.ItemId),
                ValidUntil = source.ValidUntil
            };
        }
        [Obsolete("Use Libraries.Core.Misc.TypeConversionExtensions. Obsolete warning since 2021-11-08.")]
        public static Guid? ToNullableGuid(this string source)
        {
            if (source == null) return null;
            return ToGuid(source);
        }
        
        [Obsolete("Use Libraries.Core.Misc.TypeConversionExtensions. Obsolete warning since 2021-11-08.")]
        public static Guid ToGuid(this string source)
        {
            InternalContract.RequireNotNull(source, nameof(source));
            var success = Guid.TryParse(source, out var valueAsGuid);
            InternalContract.Require(success, $"Could not parse parameter {nameof(source)} ({source}) of type {nameof(String)} into type {nameof(Guid)}.");
            return valueAsGuid;
        }
        
        [Obsolete("Use Libraries.Core.Misc.TypeConversionExtensions. Obsolete warning since 2021-11-08.")]
        public static string ToLowerCaseString(this Guid source)
        {
            return source.ToString().ToLowerInvariant();
        }
        
        [Obsolete("Use Libraries.Core.Misc.TypeConversionExtensions. Obsolete warning since 2021-11-08.")]
        public static string ToLowerCaseString(this Guid? source)
        {
            return source?.ToString().ToLowerInvariant();
        }
        
        [Obsolete("Use Libraries.Core.Misc.TypeConversionExtensions. Obsolete warning since 2021-11-08.")]
        public static int ToInt(this string source)
        {
            InternalContract.RequireNotNull(source, nameof(source));
            var success = int.TryParse(source, out var valueAsInt);
            InternalContract.Require(success, $"Could not parse parameter {nameof(source)} ({source}) of type {nameof(String)} into type {nameof(Int32)}.");
            return valueAsInt;
        }
        
        [Obsolete("Use Libraries.Core.Misc.TypeConversionExtensions. Obsolete warning since 2021-11-08.")]
        public static T? ToNullableEnum<T>(this string source) where T : struct
        {
            InternalContract.Require(typeof(T).IsEnum, $"The generic type {typeof(T).Name} must be an enum.");
            if (source == null) return null;
            return source.ToEnum<T>();
        }
        
        [Obsolete("Use Libraries.Core.Misc.TypeConversionExtensions. Obsolete warning since 2021-11-08.")]
        public static T ToEnum<T>(this string source) where T : struct
        {
            InternalContract.RequireNotNull(source, nameof(source));
            InternalContract.Require(typeof(T).IsEnum, $"The generic type {typeof(T).Name} must be an enum.");
            if (!Enum.IsDefined(typeof(T), source))
            {
                throw new FulcrumContractException(
                    $"The value {source} is not defined in the enumeration {nameof(T)}.");
            }
            var success = Enum.TryParse<T>(source, true, out var valueAsEnum);
            InternalContract.Require(success, $"Could not parse parameter {nameof(source)} ({source}) of type {nameof(String)} into type {nameof(T)}.");
            return valueAsEnum;
        }
        
        [Obsolete("Use Libraries.Core.Misc.TypeConversionExtensions. Obsolete warning since 2021-11-08.")]
        public static T? ToNullableEnum<T>(this int? source) where T : struct
        {
            InternalContract.Require(typeof(T).IsEnum, $"The generic type {typeof(T).Name} must be an enum.");
            if (source == null) return null;
            return source.Value.ToEnum<T>();
        }
        
        [Obsolete("Use Libraries.Core.Misc.TypeConversionExtensions. Obsolete warning since 2021-11-08.")]
        public static T ToEnum<T>(this int source) where T : struct
        {
            InternalContract.Require(typeof(T).IsEnum, $"The generic type {typeof(T).Name} must be an enum.");
            if (!Enum.IsDefined(typeof(T), source))
            {
                throw new FulcrumContractException(
                    $"The value {source} is not defined in the enumeration {nameof(T)}.");
            }

            return (T)Enum.ToObject(typeof(T), source);
        }

        /// <summary>
        /// Map an id between two types.
        /// </summary>
        /// <param name="source">The id to map.</param>
        /// <typeparam name="TTarget">The target type.</typeparam>
        /// <typeparam name="TSource">The source type.</typeparam>
        /// <exception cref="FulcrumNotImplementedException">Thrown if the type was not recognized. Please add that type to the class <see cref="MapperHelper"/>.</exception>
        [Obsolete("Use Libraries.Core.Misc.TypeConversionExtensions. Obsolete warning since 2021-11-08.")]
        public static TTarget MapToType<TTarget, TSource>(TSource source)
        {
            var sourceType = typeof(TSource);
            var targetType = typeof(TTarget);
            if (targetType == typeof(string))
            {
                if (source == null) return (TTarget)(object)null;
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
                if (source == null) return (TTarget)(object)null;
                return (TTarget)(object)source.ToString().ToGuid();
            }
            if (targetType == typeof(int) || targetType == typeof(int?))
            {
                if (targetType == typeof(int))
                {
                    InternalContract.RequireNotNull(source, nameof(source));
                }
                if (source == null) return (TTarget)(object)null;
                return (TTarget)(object)source.ToString().ToInt();
            }
            throw new FulcrumNotImplementedException($"There is currently no rule on how to convert an id from type {sourceType.Name} to type {targetType.Name}.");
        }

        /// <summary>
        /// Map an id between two types.
        /// </summary>
        /// <param name="source">The id to map.</param>
        /// <typeparam name="TTarget">The target type.</typeparam>
        /// <typeparam name="TSource">The source type.</typeparam>
        /// <exception cref="FulcrumNotImplementedException">Thrown if the type was not recognized. Please add that type to the class <see cref="MapperHelper"/>.</exception>
        [Obsolete("Use Libraries.Core.Misc.TypeConversionExtensions. Obsolete warning since 2021-11-08.")]
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
        /// <exception cref="FulcrumNotImplementedException">Thrown if the type was not recognized. Please add that type to the class <see cref="MapperHelper"/>.</exception>
        [Obsolete("Use Libraries.Core.Misc.TypeConversionExtensions. Obsolete warning since 2021-11-08.")]
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
                else if (sourceType == typeof(int) || sourceType == typeof(int?))
                {
                    return ((int)(object)source).ToEnum<TTarget>();
                }
            }
            throw new FulcrumNotImplementedException($"There is currently no rule on how to convert an id from type {sourceType.Name} to type {targetType.Name}.");
        }

        /// <summary>
        /// Map an id between two types.
        /// </summary>
        /// <param name="source">The id to map.</param>
        /// <exception cref="FulcrumNotImplementedException">Thrown if the type was not recognized. Please add that type to the class <see cref="MapperHelper"/>.</exception>
        [Obsolete("Not needed since we introduced DependentToMaster. Obsolete since 2021-08-27.")]
        public static SlaveToMasterId<TTarget> MapToType<TTarget, TSource>(SlaveToMasterId<TSource> source)
        {
            if (source == null) return null;
            var targetMasterId = MapToType<TTarget, TSource>(source.MasterId);
            var targetSlaveId = MapToType<TTarget, TSource>(source.SlaveId);
            return new SlaveToMasterId<TTarget>(targetMasterId, targetSlaveId);
        }
    }
}
