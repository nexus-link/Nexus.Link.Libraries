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

        public static Guid ToGuid(this string source)
        {
            var success = Guid.TryParse(source, out var valueAsGuid);
            InternalContract.Require(success, $"Could not parse parameter {nameof(source)} ({source}) of type {nameof(String)} into type {nameof(Guid)}.");
            return valueAsGuid;
        }

        public static int ToInt(this string source)
        {
            var success = int.TryParse(source, out var valueAsInt);
            InternalContract.Require(success, $"Could not parse parameter {nameof(source)} ({source}) of type {nameof(String)} into type {nameof(Int32)}.");
            return valueAsInt;
        }

        public static T ToEnum<T>(this string source) where T : struct
        {
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
        /// <exception cref="FulcrumNotImplementedException">Thrown if the type was not recognized. Please add that type to the class <see cref="MapperHelper"/>.</exception>
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
        /// <exception cref="FulcrumNotImplementedException">Thrown if the type was not recognized. Please add that type to the class <see cref="MapperHelper"/>.</exception>
        public static TTarget? MapToStructOrNull<TTarget, TSource>(TSource source) where TTarget : struct
        {
            if (source == null) return null;
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

        /// <summary>
        /// Map an id between two types.
        /// </summary>
        /// <param name="source">The id to map.</param>
        /// <typeparam name="TTarget">The target type.</typeparam>
        /// <typeparam name="TSource">The source type.</typeparam>
        /// <exception cref="FulcrumNotImplementedException">Thrown if the type was not recognized. Please add that type to the class <see cref="MapperHelper"/>.</exception>
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

        /// <summary>
        /// Map a lock between two types.
        /// </summary>
        /// <param name="source">The lock to map.</param>
        /// <exception cref="FulcrumNotImplementedException">Thrown if the type was not recognized. Please add that type to the class <see cref="MapperHelper"/>.</exception>

        public static Lock<TTarget> MapToType<TTarget, TSource>(Lock<TSource> source)
        {
            return new Lock<TTarget>
            {
                Id = MapToType<TTarget, TSource>(source.Id),
                ItemId = MapToType<TTarget, TSource>(source.ItemId),
                ValidUntil = source.ValidUntil
            };
        }
    }
}
