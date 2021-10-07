using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Libraries.Core.Storage.Model;

namespace Nexus.Link.Libraries.Crud.Interfaces
{
    /// <summary>
    /// Functionality for persisting objects that has no life of their own, but are only relevant with their master.
    /// Examples: A list of rows on an invoice, a list of attributes of an object, the contact details of a person.
    /// </summary>
    public interface ICreateDependentWithSpecifiedId<in TModel, in TId, in TDependentId> : ICreateDependentWithSpecifiedId<TModel, TModel, TId, TDependentId>
    {
    }

    /// <summary>
    /// Functionality for persisting objects that has no life of their own, but are only relevant with their master.
    /// Examples: A list of rows on an invoice, a list of attributes of an object, the contact details of a person.
    /// </summary>
    public interface ICreateDependentWithSpecifiedId<in TModelCreate, in TModel, in TId, in TDependentId> : ICrudableDependent<TModel, TId, TDependentId>
        where TModel : TModelCreate
    {
        /// <summary>
        /// Same as <see cref="ICreateDependent{TModelCreate,TModel,TId,TDependentId}.CreateAsync"/>, but you can specify the new id.
        /// </summary>
        /// <param name="masterId">The id of the master for this dependent.</param>
        /// <param name="dependentId">The proposed id for this dependent.</param>
        /// <param name="item">The item to create in storage.</param>
        /// <param name="cancellationToken ">Propagates notification that operations should be canceled</param>
        /// <returns>The newly created item.</returns>
        Task CreateWithSpecifiedIdAsync(TId masterId, TDependentId dependentId, TModelCreate item, CancellationToken cancellationToken  = default);
    }
}
