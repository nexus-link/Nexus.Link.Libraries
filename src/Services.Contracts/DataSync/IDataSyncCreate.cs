using Nexus.Link.Libraries.Crud.Interfaces;

namespace Nexus.Link.Services.Contracts.DataSync
{
    /// <summary>
    /// The optional Create method for data sync.
    /// </summary>
    public interface IDataSyncCreate<T> : ICreate<T, string>
    {
    }
}