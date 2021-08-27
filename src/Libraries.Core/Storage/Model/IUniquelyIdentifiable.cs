namespace Nexus.Link.Libraries.Core.Storage.Model
{
    /// <summary>
    /// Properties required for an item to be stored in a "table".
    /// </summary>
    /// <typeparam name="TId">The type for the property <see cref="Id"/>.</typeparam>
    public interface IUniquelyIdentifiable<TId>
    {
        /// <summary>
        /// A unique identifier for the item.
        /// </summary>
        TId Id { get; set; }
    }

    /// <summary>
    /// Properties required for a dependent item to be stored in a "table".
    /// </summary>
    public interface IUniquelyIdentifiable<TId, TDependentId> : IUniquelyIdentifiable<TId>
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
