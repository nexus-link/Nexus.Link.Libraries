using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Libraries.Core.Storage.Model;

namespace Nexus.Link.Libraries.Crud.Interfaces
{
    /// <summary>
    /// Functionality for persisting many-to-many relations.
    /// </summary>
    /// <typeparam name="TManyToManyModel">The type of objects that are returned from persistant storage.</typeparam>
    /// <typeparam name="TReferenceModel1"></typeparam>
    /// <typeparam name="TReferenceModel2"></typeparam>
    /// <typeparam name="TId"></typeparam>
    public interface IManyToMany<TManyToManyModel, TReferenceModel1, TReferenceModel2, TId> :
        IManyToMany<TManyToManyModel, TManyToManyModel, TReferenceModel1, TReferenceModel2, TId>
    {
    }

    /// <summary>
    /// Functionality for persisting many-to-many relations.
    /// </summary>
    public interface IManyToMany<in TManyToManyModelCreate, TManyToManyModel, TReferenceModel1, TReferenceModel2, TId>
        where TManyToManyModel : TManyToManyModelCreate
    {
        #region Access the many-to-many items themselves by Reference 1
        /// <summary>
        /// Find all items with reference 1 set to <paramref name="id"/>.
        /// </summary>
        /// <param name="id">The specific foreign key value to read the items for.</param>
        /// <param name="offset">The number of items that will be skipped in result.</param>
        /// <param name="limit">The maximum number of items to return.</param>
        /// <param name="token">Propagates notification that operations should be canceled</param>
        Task<PageEnvelope<TManyToManyModel>> ReadByReference1WithPagingAsync(TId id, int offset, int? limit = null, CancellationToken token = default);

        /// <summary>
        /// Find all items reference 1 set to <paramref name="id"/>.
        /// </summary>
        /// <param name="id">The specific foreign key value to read the items for.</param>
        /// <param name="limit">The maximum number of items to return.</param>
        /// <param name="token">Propagates notification that operations should be canceled</param>
        Task<IEnumerable<TManyToManyModel>> ReadByReference1Async(TId id, int limit = int.MaxValue, CancellationToken token = default);

        /// <summary>
        /// Delete all items reference 1 set to <paramref name="id"/>.
        /// </summary>
        /// <param name="id">The specific foreign key value to delete the items for.</param>
        /// <param name="token">Propagates notification that operations should be canceled</param>
        Task DeleteByReference1Async(TId id, CancellationToken token = default);
        #endregion

        #region Access the referenced items by Reference 1
        /// <summary>
        /// Find all referenced items with foreign key 1 set to <paramref name="id"/>.
        /// </summary>
        /// <param name="id">The specific foreign key value to read the referenced items for.</param>
        /// <param name="offset">The number of items that will be skipped in result.</param>
        /// <param name="limit">The maximum number of items to return.</param>
        /// <param name="token">Propagates notification that operations should be canceled</param>
        Task<PageEnvelope<TReferenceModel2>> ReadReferencedItemsByReference1WithPagingAsync(TId id, int offset, int? limit = null, CancellationToken token = default);

        /// <summary>
        /// Find all referenced items with foreign key 1 set to <paramref name="id"/>.
        /// </summary>
        /// <param name="id">The specific foreign key value to read the referenced items for.</param>
        /// <param name="limit">The maximum number of items to return.</param>
        /// <param name="token">Propagates notification that operations should be canceled</param>
        Task<IEnumerable<TReferenceModel2>> ReadReferencedItemsByReference1Async(TId id, int limit = int.MaxValue, CancellationToken token = default);

        /// <summary>
        /// Delete all referenced items where foreign key 1 is set to <paramref name="id"/>.
        /// </summary>
        Task DeleteReferencedItemsByReference1(TId id, CancellationToken token = default);
        #endregion

        #region Access the many-to-many items themselves by Reference 2

        /// <summary>
        /// Find all items with reference 2 set to <paramref name="id"/>.
        /// </summary>
        /// <param name="id">The specific foreign key value to read the items for.</param>
        /// <param name="offset">The number of items that will be skipped in result.</param>
        /// <param name="limit">The maximum number of items to return.</param>
        /// <param name="token">Propagates notification that operations should be canceled</param>
        Task<PageEnvelope<TManyToManyModel>> ReadByReference2WithPagingAsync(TId id, int offset, int? limit = null, CancellationToken token = default);

        /// <summary>
        /// Find all items reference 2 set to <paramref name="id"/>.
        /// </summary>
        /// <param name="id">The specific foreign key value to read the items for.</param>
        /// <param name="limit">The maximum number of items to return.</param>
        /// <param name="token">Propagates notification that operations should be canceled</param>
        Task<IEnumerable<TManyToManyModel>> ReadByReference2Async(TId id, int limit = int.MaxValue, CancellationToken token = default);

        /// <summary>
        /// Delete all items reference 2 set to <paramref name="id"/>.
        /// </summary>
        /// <param name="id">The specific foreign key value to delete the items for.</param>
        /// <param name="token">Propagates notification that operations should be canceled</param>
        Task DeleteByReference2Async(TId id, CancellationToken token = default);
        #endregion

        #region Access the referenced items by Reference 2
        /// <summary>
        /// Find all referenced items with foreign key 2 set to <paramref name="id"/>.
        /// </summary>
        /// <param name="id">The specific foreign key value to read the referenced items for.</param>
        /// <param name="offset">The number of items that will be skipped in result.</param>
        /// <param name="limit">The maximum number of items to return.</param>
        /// <param name="token">Propagates notification that operations should be canceled</param>
        Task<PageEnvelope<TReferenceModel1>> ReadReferencedItemsByReference2WithPagingAsync(TId id, int offset, int? limit = null, CancellationToken token = default);

        /// <summary>
        /// Find all referenced items with foreign key 2 set to <paramref name="id"/>.
        /// </summary>
        /// <param name="id">The specific foreign key value to read the referenced items for.</param>
        /// <param name="limit">The maximum number of items to return.</param>
        /// <param name="token">Propagates notification that operations should be canceled</param>
        Task<IEnumerable<TReferenceModel1>> ReadReferencedItemsByReference2Async(TId id, int limit = int.MaxValue, CancellationToken token = default);

        /// <summary>
        /// Delete all referenced items where foreign key 2 is set to <paramref name="id"/>.
        /// </summary>
        Task DeleteReferencedItemsByReference2(TId id, CancellationToken token = default);
        #endregion
    }
}
