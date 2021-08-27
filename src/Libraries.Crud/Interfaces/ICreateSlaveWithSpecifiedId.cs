using System;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Libraries.Core.Storage.Model;

namespace Nexus.Link.Libraries.Crud.Interfaces
{
    /// <summary>
    /// Functionality for persisting objects that has no life of their own, but are only relevant with their master.
    /// Examples: A list of rows on an invoice, a list of attributes of an object, the contact details of a person.
    /// </summary>
    [Obsolete("Use ICreateSlaveWithSpecifiedId. Obsolete since 2021-08-27.")]
    public interface ICreateSlaveWithSpecifiedId<TModel, in TId> : ICreateSlaveWithSpecifiedId<TModel, TModel, TId>
    {
    }

    /// <summary>
    /// Functionality for persisting objects that has no life of their own, but are only relevant with their master.
    /// Examples: A list of rows on an invoice, a list of attributes of an object, the contact details of a person.
    /// </summary>
    [Obsolete("Use ICreateSlaveWithSpecifiedId. Obsolete since 2021-08-27.")]
    public interface ICreateSlaveWithSpecifiedId<in TModelCreate, TModel, in TId> : ICrudable<TModel, TId>
        where TModel : TModelCreate
    {
        /// <summary>
        /// Same as <see cref="Nexus.Link.Libraries.Crud.Interfaces.ICreateSlave{TModelCreate,TModel,TId}.CreateAsync"/>, but you can specify the new id.
        /// </summary>
        /// <param name="masterId">The id of the master for this slave.</param>
        /// <param name="slaveId">The proposed id for this slave.</param>
        /// <param name="item">The item to create in storage.</param>
        /// <param name="token">Propagates notification that operations should be canceled</param>
        /// <returns>The newly created item.</returns>
        Task CreateWithSpecifiedIdAsync(TId masterId, TId slaveId, TModelCreate item, CancellationToken token = default);

        /// <summary>
        /// Same as <see cref="Nexus.Link.Libraries.Crud.Interfaces.ICreateSlaveAndReturn{TModelCreate,TModel,TId}.CreateAndReturnAsync"/>, but you can specify the new id.
        /// </summary>
        /// <param name="masterId">The id of the master for this slave.</param>
        /// <param name="slaveId">The proposed id for this slave.</param>
        /// <param name="item">The item to store.</param>
        /// <param name="token">Propagates notification that operations should be canceled</param>
        /// <returns>The new item as it was saved, see remarks below.</returns>
        /// <remarks>
        /// If the returned type implements <see cref="IUniquelyIdentifiable{TId}"/>, then the <see cref="IUniquelyIdentifiable{TId}.Id"/> is updated with the new id. 
        /// If it implements <see cref="IOptimisticConcurrencyControlByETag"/>, then the <see cref="IOptimisticConcurrencyControlByETag.Etag"/> is updated..
        /// </remarks>
        /// <seealso cref="IOptimisticConcurrencyControlByETag"/>
        /// <seealso cref="IUniquelyIdentifiable{TId}"/>
        Task<TModel> CreateWithSpecifiedIdAndReturnAsync(TId masterId, TId slaveId, TModelCreate item, CancellationToken token = default);
    }
}
