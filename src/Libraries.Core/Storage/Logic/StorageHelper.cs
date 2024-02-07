using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.EntityAttributes;
using Nexus.Link.Libraries.Core.Storage.Logic.SequentialGuid;
using Nexus.Link.Libraries.Core.Storage.Model;

namespace Nexus.Link.Libraries.Core.Storage.Logic
{
    /// <summary>
    /// Helper methods for Storage
    /// </summary>
    public static class StorageHelper
    {
        private static readonly SpinLock SpinLock = new SpinLock(false);
        private static GuidOptimization _optimization = GuidOptimization.None;
        private static IGuidGenerator _guidGenerator = null;

        public static IGuidGenerator GuidGenerator => _guidGenerator;

        /// <summary>
        /// Gets or sets the type of sequential <see cref="Guid"/> that should be generated.
        /// </summary>
        public static GuidOptimization Optimization
        {
            get => _optimization;
            set
            {
                if (_optimization == value)
                    return;
                var lockTaken = false;
                SpinLock.Enter(ref lockTaken);
                _optimization = value;
                switch (_optimization)
                {
                    case GuidOptimization.None:
                        _guidGenerator = null;
                        break;
                    case GuidOptimization.SqlServer:
                        _guidGenerator = new SqlServerGuidGenerator();
                        break;
                    case GuidOptimization.SqlServerWithProcessId:
                        _guidGenerator = new SqlServerGuidGenerator();
                        break;
                    case GuidOptimization.MySql:
                    case GuidOptimization.RavenDb:
                        _guidGenerator = new GenericGuidGenerator(SequentialGuidType.AsString);
                        break;
                    case GuidOptimization.Oracle:
                        _guidGenerator = new GenericGuidGenerator(SequentialGuidType.AsBinary);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
                if (lockTaken)
                    SpinLock.Exit();
            }
        }

        /// <summary>
        /// Creates a sequential GUID according to <see cref="StorageHelper.Optimization"/> ordering rules.
        /// </summary>
        public static Guid NewSequentialGuid()
        {
            return StorageHelper.Optimization is GuidOptimization.None ? System.Guid.NewGuid() : StorageHelper.CreateNewId<Guid>();
        }

        /// <summary>
        /// Create a new Id of type <see cref="string"/> or type <see cref="Guid"/>.
        /// </summary>
        /// <typeparam name="TId"></typeparam>
        /// <returns></returns>
        public static TId CreateNewId<TId>()
        {
            var id = default(TId);
            if (typeof(TId) == typeof(Guid))
            {
                if (Optimization is GuidOptimization.None)
                    id = (dynamic)Guid.NewGuid();
                else
                    id = (dynamic)_guidGenerator.NewSequentialGuid();
            }
            else if (typeof(TId) == typeof(string))
            {

                if (Optimization is GuidOptimization.None)
                {
                    id = (dynamic)Guid.NewGuid().ToString();
                }
                else
                {
                    if (_guidGenerator is GenericGuidGenerator { SequentialGuidType: SequentialGuidType.AsString } generator)
                        id = (dynamic)generator.NewSequentialGuid().ToString();
                    else
                        id = (dynamic)new GenericGuidGenerator(SequentialGuidType.AsString).NewSequentialGuid().ToString();
                }
            }
            else
            {
                FulcrumAssert.Fail(null,
                    $"{nameof(CreateNewId)} can handle Guid and string as type for Id, but it can't handle {typeof(TId)}.");
            }
            return id;
        }

        /// <summary>
        /// A generic method for deep copying.
        /// </summary>
        /// <param name="source">The object that should be copied.</param>
        /// <returns>A copied object.</returns>
        public static T DeepCopy<T>(T source)
        {
            return JsonConvert.DeserializeObject<T>(JsonConvert.SerializeObject(source));
        }

        /// <summary>
        /// A generic method for deep copying.
        /// </summary>
        /// <param name="source">The object that should be copied.</param>
        /// <returns>A copied object.</returns>
        public static TTarget DeepCopy<TTarget, TSource>(TSource source)
        where TTarget : TSource
        {
            return JsonConvert.DeserializeObject<TTarget>(JsonConvert.SerializeObject(source));
        }

        /// <summary>
        /// If <paramref name="item"/> implements <see cref="IOptimisticConcurrencyControlByETag"/>
        /// then the Etag of the item is set to a new value.
        /// </summary>
        [Obsolete("Please use the extension method TrySetOptimisticConcurrencyControl(), i.e. item.TrySetOptimisticConcurrencyControl(). Obsolete since 2022-01-28.")]
        public static void MaybeCreateNewEtag<TModel>(TModel item)
        {
            item.TrySetOptimisticConcurrencyControl();
        }

        /// <summary>
        /// If <paramref name="item"/> implements <see cref="IOptimisticConcurrencyControlByETag"/>
        /// then the Etag of the item is set to a new value.
        /// </summary>
        public static bool TrySetOptimisticConcurrencyControl<TModel>(this TModel item, string value = null)
        {
            if (value == null) value = Guid.NewGuid().ToString();
            if (item is IOptimisticConcurrencyControlByETag optimistic)
            {
                optimistic.Etag = value;
                return true;
            }

            return item.TrySetValueForPropertyWithCustomAttribute<Hint.OptimisticConcurrencyControlAttribute>(value);
        }

        /// <summary>
        /// If <paramref name="item"/> implements <see cref="IUniquelyIdentifiable{TId}"/>
        /// then the Id of the item is set.
        /// </summary>
        [Obsolete("Please use the extension method TrySetPrimaryKey(), i.e. item.MaybeSetPrimaryId(id). Obsolete since 2022-01-28.")]
        public static void MaybeSetId<TId, TModel>(TId id, TModel item)
        {
            item.TrySetPrimaryKey(id);
        }

        /// <summary>
        /// Try to find a way to set the primary key property for the <paramref name="item"/>.
        /// </summary>
        public static bool TrySetPrimaryKey<TModel, TId>(this TModel item, TId id)
        {
            if (item is IUniquelyIdentifiable<TId> identifiable)
            {
                identifiable.Id = id;
                return true;
            }

            return item.TrySetValueForPropertyWithCustomAttribute<Hint.PrimaryKeyAttribute>(id);
        }

        /// <summary>
        /// Try to find a way to set the updated-at property for the <paramref name="item"/>.
        /// </summary>
        public static bool TrySetUpdatedAt<TModel>(this TModel item, DateTimeOffset timeStamp)
        {
            if (item is ITimeStamped timeStamped)
            {
                timeStamped.RecordUpdatedAt = timeStamp;
                return true;
            }

            return item.TrySetValueForPropertyWithCustomAttribute<Hint.RecordUpdatedAtAttribute>(timeStamp);
        }

        /// <summary>
        /// Try to find a way to set the created-at property for the <paramref name="item"/>.
        /// </summary>
        public static bool TrySetCreatedAt<TModel>(this TModel item, DateTimeOffset timeStamp)
        {
            if (item is ITimeStamped timeStamped)
            {
                timeStamped.RecordCreatedAt = timeStamp;
                return true;
            }

            return item.TrySetValueForPropertyWithCustomAttribute<Hint.RecordCreatedAtAttribute>(timeStamp);
        }

        /// <summary>
        /// If <paramref name="item"/> implements <see cref="IUniquelyIdentifiable{TId}"/>
        /// then the Id of the item is set.
        /// </summary>
        public static TId GetPrimaryKey<TModel, TId>(this TModel item)
        {
            item.TryGetPrimaryKey<TModel, TId>(out var id);
            return id;
        }

        /// <summary>
        /// Returns true if a primary key could be found and the value is put into <paramref name="id"/>.
        /// </summary>
        public static bool TryGetPrimaryKey<TModel, TId>(this TModel item, out TId id)
        {
            if (item is IUniquelyIdentifiable<TId> identifiable)
            {
                id = identifiable.Id;
                return true;
            }

            var isSuccess = item.TryGetValueForPropertyWithCustomAttribute<Hint.PrimaryKeyAttribute>(out var idAsObject);
            id = isSuccess ? (TId)idAsObject : default;
            return isSuccess;
        }

        /// <summary>
        /// Returns true if a primary key could be found and the value is put into <paramref name="id"/>.
        /// </summary>
        public static bool TryGetOptimisticConcurrencyControl<TModel>(this TModel item, out string eTag)
        {
            if (item is IOptimisticConcurrencyControlByETag optimistic)
            {
                eTag = optimistic.Etag;
                return true;
            }

            var isSuccess = item.TryGetValueForPropertyWithCustomAttribute<Hint.OptimisticConcurrencyControlAttribute>(out var eTagAsObject);
            eTag = isSuccess ? (string)eTagAsObject : null;
            return isSuccess;
        }

        public static PropertyInfo GetPrimaryKeyProperty<TModel, TId>()
        {
            if (typeof(IUniquelyIdentifiable<TId>).IsAssignableFrom(typeof(TModel)))
            {
                return typeof(IUniquelyIdentifiable<TId>).GetProperty(nameof(IUniquelyIdentifiable<TId>.Id));
            }
            if (!typeof(TModel).IsClass) return null;
            return typeof(TModel).GetPropertyWithCustomAttribute<Hint.PrimaryKeyAttribute>();
        }

        /// <summary>
        /// If <paramref name="item"/> implements <see cref="IUniquelyIdentifiable{TId}"/>
        /// then the Id of the item is set.
        /// </summary>
        public static void MaybeSetMasterAndDependentId<TModel, TId, TDependentId>(TId masterId, TDependentId dependentId, TModel item)
        {
            if (item is IUniquelyIdentifiableDependent<TId, TDependentId> combinedId)
            {
                combinedId.MasterId = masterId;
                combinedId.DependentId = dependentId;
            }
        }

        /// <summary>
        /// If <paramref name="item"/> implements <see cref="ITimeStamped"/>
        /// then the <see cref="ITimeStamped.RecordUpdatedAt"/> is set. If <paramref name="updateCreatedToo"/> is true, 
        /// then the <see cref="ITimeStamped.RecordCreatedAt"/> is also set.
        /// </summary>
        /// <param name="item">The item that will be affected.</param>
        /// <param name="updateCreatedToo">True means that we should update the create property too.</param>
        /// <param name="timeStamp">Optional time stamp to use when setting the time properties. If null, then 
        /// <see cref="DateTimeOffset.Now"/> will be used.</param>
        public static void MaybeUpdateTimeStamps<TModel>(TModel item, bool updateCreatedToo, DateTimeOffset? timeStamp = null)
        {
            timeStamp = timeStamp ?? DateTimeOffset.UtcNow;
            item.TrySetUpdatedAt(timeStamp.Value);
            if (updateCreatedToo) item.TrySetCreatedAt(timeStamp.Value);
        }

        /// <summary>
        /// Helper method to convert from one parameter type to another.
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static T ConvertToParameterType<T>(object source)
        {
            object referenceIdAsObject = source;
            try
            {
                var target = (T)referenceIdAsObject;
                return target;
            }
            catch (Exception e)
            {
                InternalContract.Fail(
                    $"The value \"{source}\" of type {source.GetType().Name} can't be converted into type {typeof(T).Name}:\r" +
                    $"{e.Message}");
                // We should not end up at this line, but the compiler think that we can, so we add a throw here.
                throw;
            }
        }

        /// <summary>
        /// Read pages until the <paramref name="limit"/> number of items have been read (or the pages are exhausted) and return the result.
        /// </summary>
        /// <param name="readMethodDelegateAsync">A method that returns one page at the time</param>
        /// <param name="limit">The maximum number of items to return.</param>
        /// <param name="token">Propagates notification that operations should be canceled</param>
        /// <typeparam name="TModel">The type of the items.</typeparam>
        public static async Task<IEnumerable<TModel>> ReadPagesAsync<TModel>(
            PageEnvelopeEnumeratorAsync<TModel>.ReadMethodDelegate readMethodDelegateAsync, int limit = int.MaxValue, CancellationToken token = default)
        {
            var result = new List<TModel>();
            var enumerator = new PageEnvelopeEnumeratorAsync<TModel>(readMethodDelegateAsync, token);
            var count = 0;
            while (count < limit && await enumerator.MoveNextAsync())
            {
                result.Add(enumerator.Current);
                count++;
            }

            // Paging can result in duplicates

            if (GetUnique<Guid>(out var unique)) return unique;
            if (GetUnique<string>(out unique)) return unique;
            if (GetUnique<int>(out unique)) return unique;

            var uniqueSet = new HashSet<TModel>();
            foreach (var item in result)
            {
                uniqueSet.Add(item);
            }

            return uniqueSet;

            bool GetUnique<T>(out IEnumerable<TModel> enumerable)
            {
                enumerable = Enumerable.Empty<TModel>();
                if (!typeof(IUniquelyIdentifiable<T>).IsAssignableFrom(typeof(TModel))) return false;
                var ids = new HashSet<T>();
                var uniqueList = new List<TModel>();
                foreach (var item in result)
                {
                    var identifiable = item as IUniquelyIdentifiable<T>;
                    if (identifiable == null) continue;
                    if (ids.Contains(identifiable.Id)) continue;
                    ids.Add(identifiable.Id);
                    uniqueList.Add(item);
                }

                enumerable = uniqueList;
                return true;

            }
        }
    }
}
