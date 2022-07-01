namespace Nexus.Link.Libraries.Core.Storage.Model
{
    /// <summary>
    /// Properties required for a dependent item to be stored in a "table".
    /// </summary>
    public interface IUniquelyIdentifiableDependentWithUniqueId<TId, TDependentId> : IUniquelyIdentifiableDependent<TId, TDependentId>, IUniquelyIdentifiable<TId>
    {
    }
}