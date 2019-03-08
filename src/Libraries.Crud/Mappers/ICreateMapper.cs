namespace Nexus.Link.Libraries.Crud.Mappers
{

    /// <summary>
    /// Methods for mapping data between the client and server models.
    /// </summary>
    public interface ICreateMapper<in TClientModelCreate, TClientModel, TServerModel> : IMappable<TClientModel, TServerModel>
    {
        /// <summary>
        /// Map fields to the server
        /// </summary>
        /// <param name="source">The client object that we should map to a server record.</param>
        /// <returns>A new server record with the mapped values from <paramref name="source"/>.</returns>
        TServerModel MapToServer(TClientModelCreate source);
    }
}