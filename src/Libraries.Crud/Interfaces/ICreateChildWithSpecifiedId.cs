using System.Threading;
using System.Threading.Tasks;

namespace Nexus.Link.Libraries.Crud.Interfaces
{
    /// <summary>
    /// Functionality for persisting objects that has no life of their own, but are only relevant with their parent.
    /// Examples: A list of rows on an invoice, a list of attributes of an object, the contact details of a person.
    /// </summary>
    public interface ICreateChildWithSpecifiedId<in TModel, in TId> : ICreateChildWithSpecifiedId<TModel, TModel, TId>
    {
    }

    /// <summary>
    /// Functionality for persisting objects that has no life of their own, but are only relevant with their parent.
    /// Examples: A list of rows on an invoice, a list of attributes of an object, the contact details of a person.
    /// </summary>
    public interface ICreateChildWithSpecifiedId<in TModelCreate, in TModel, in TId> : ICrudable<TModel, TId>
        where TModel : TModelCreate
    {
        /// <summary>
        /// Same as <see cref="ICreateChild{TModelCreate,TModel,TId}.CreateChildAsync"/>, but you can specify the new id.
        /// </summary>
        /// <param name="parentId">The id of the parent for this child.</param>
        /// <param name="childId">The proposed id for this child.</param>
        /// <param name="item">The item to create in storage.</param>
        /// <param name="token">Propagates notification that operations should be canceled</param>
        /// <returns>The newly created item.</returns>
        Task CreateChildWithSpecifiedIdAsync(TId parentId, TId childId, TModelCreate item, CancellationToken token = default);
    }
}
