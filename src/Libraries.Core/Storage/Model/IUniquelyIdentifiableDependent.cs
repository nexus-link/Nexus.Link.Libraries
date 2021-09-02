namespace Nexus.Link.Libraries.Core.Storage.Model
{
    /// <summary>
    /// Properties required for a dependent item to be stored in a "table".
    /// </summary>
    public interface IUniquelyIdentifiableDependent<TId, TDependentId>
    {
        /// <summary>
        /// The master id for the type.
        /// </summary>
        TId MasterId { get; set; }

        /// <summary>
        /// The master id for the type.
        /// </summary>
        TDependentId DependentId { get; set; }
    }
}