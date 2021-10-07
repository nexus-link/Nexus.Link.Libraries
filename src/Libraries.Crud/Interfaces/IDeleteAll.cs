using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Libraries.Core.Error.Logic;

namespace Nexus.Link.Libraries.Crud.Interfaces
{
    /// <summary>
    /// Delete items./>.
    /// </summary>
    public interface IDeleteAll : ICrudable
    {
        /// <summary>
        /// Delete all the items from storage.
        /// </summary>
        /// <param name="cancellationToken ">Propagates notification that operations should be canceled</param>
        /// <remarks>
        /// The implementor of this method can decide that it is not a valid method to expose.
        /// In that case, the method should throw a <see cref="FulcrumNotImplementedException"/>.
        /// </remarks>
        Task DeleteAllAsync(CancellationToken cancellationToken  = default);
    }
}
