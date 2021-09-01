using System.Threading;
using System.Threading.Tasks;

namespace Nexus.Link.Libraries.Crud.Interfaces
{
    /// <summary>
    /// Functionality for persisting objects that has no life of their own, but are only relevant with their master.
    /// Examples: A list of rows on an invoice, a list of attributes of an object, the contact details of a person.
    /// </summary>
    public interface ICreateDependent<in TModel, in TId, TDependentId> : ICreateDependent<TModel, TModel, TId, TDependentId>
    {
    }

    /// <summary>
    /// Functionality for persisting objects that has no life of their own, but are only relevant with their master.
    /// Examples: A list of rows on an invoice, a list of attributes of an object, the contact details of a person.
    /// </summary>
    public interface ICreateDependent<in TModelCreate, in TModel, in TId, TDependentId> : ICrudableDependent<TModel, TId, TDependentId>
        where TModel : TModelCreate
    {
        /// <summary>
        /// Creates a new item in storage and returns the new Id.
        /// </summary>
        /// <param name="masterId">The master that the dependent belongs to.</param>
        /// <param name="item">The item to store.</param>
        /// <param name="token">Propagates notification that operations should be canceled</param>
        /// <returns>The new id for the created object.</returns>
        Task<TDependentId> CreateAsync(TId masterId, TModelCreate item, CancellationToken token = default);
    }
}
