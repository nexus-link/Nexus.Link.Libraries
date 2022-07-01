using System;
using System.Threading;
using System.Threading.Tasks;

namespace Nexus.Link.Libraries.Crud.Interfaces
{
    /// <summary>
    /// Functionality for persisting objects that has no life of their own, but are only relevant with their master.
    /// Examples: A list of rows on an invoice, a list of attributes of an object, the contact details of a person.
    /// </summary>
    [Obsolete("Use ICreateDependent. Obsolete since 2021-08-27.")]
    public interface ICreateSlave<TModel, TId> : ICreateSlave<TModel, TModel, TId>
    {
    }

    /// <summary>
    /// Functionality for persisting objects that has no life of their own, but are only relevant with their master.
    /// Examples: A list of rows on an invoice, a list of attributes of an object, the contact details of a person.
    /// </summary>
    [Obsolete("Use ICreateDependent. Obsolete since 2021-08-27.")]
    public interface ICreateSlave<in TModelCreate, TModel, TId> : ICrudable<TModel, TId>
        where TModel : TModelCreate
    {
        /// <summary>
        /// Creates a new item in storage and returns the new Id.
        /// </summary>
        /// <param name="masterId">The master that the slave belongs to.</param>
        /// <param name="item">The item to store.</param>
        /// <param name="cancellationToken">Propagates notification that operations should be canceled</param>
        /// <returns>The new id for the created object.</returns>
        Task<TId> CreateAsync(TId masterId, TModelCreate item, CancellationToken cancellationToken  = default);
    }
}
