using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Libraries.Core.Storage.Model;

namespace Nexus.Link.Libraries.Crud.Interfaces
{
    /// <summary>
    /// Functionality for persisting objects that has no life of their own, but are only relevant with their master.
    /// Examples: A list of rows on an invoice, a list of attributes of an object, the contact details of a person.
    /// </summary>
    public interface ICreateDependentWithSpecifiedIdAndReturn<TModel, in TId, in TDependentId> : ICreateDependentWithSpecifiedIdAndReturn<TModel, TModel, TId, TDependentId>
    {
    }

    /// <summary>
    /// Functionality for persisting objects that has no life of their own, but are only relevant with their master.
    /// Examples: A list of rows on an invoice, a list of attributes of an object, the contact details of a person.
    /// </summary>
    public interface ICreateDependentWithSpecifiedIdAndReturn<in TModelCreate, TModel, in TId, in TDependentId> : ICrudableDependent<TModel, TId, TDependentId>
        where TModel : TModelCreate
    {
        /// <summary>
        /// Same as <see cref="ICreateDependentAndReturn{TModelCreate,TModel,TDependentId}.CreateAndReturnAsync"/>, but you can specify the new id.
        /// </summary>
        /// <param name="masterId">The id of the master for this dependent.</param>
        /// <param name="dependentId">The proposed id for this dependent.</param>
        /// <param name="item">The item to store.</param>
        /// <param name="token">Propagates notification that operations should be canceled</param>
        /// <returns>The new item as it was saved, see remarks below.</returns>
        /// <remarks>
        /// If the returned type implements <see cref="IUniquelyIdentifiable{TDependentId}"/>, then the <see cref="IUniquelyIdentifiable{TDependentId}.Id"/> is updated with the new id. 
        /// If it implements <see cref="IOptimisticConcurrencyControlByETag"/>, then the <see cref="IOptimisticConcurrencyControlByETag.Etag"/> is updated..
        /// </remarks>
        /// <seealso cref="IOptimisticConcurrencyControlByETag"/>
        /// <seealso cref="IUniquelyIdentifiable{TId, TDependentId}"/>
        Task<TModel> CreateWithSpecifiedIdAndReturnAsync(TId masterId, TDependentId dependentId, TModelCreate item, CancellationToken token = default);
    }
}
