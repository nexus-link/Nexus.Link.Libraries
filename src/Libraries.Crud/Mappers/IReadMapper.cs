using System;

namespace Nexus.Link.Libraries.Crud.Mappers
{

    /// <summary>
    /// Methods for mapping data between the client and server models.
    /// </summary>
    [Obsolete("We no longer recommend to use this mapping technique. Obsolete since 2020-09-23.")]
    public interface IReadMapper<TClientModel, TServerModel> : IMappable<TClientModel, TServerModel>
    {
        /// <summary>
        /// Map fields from the server
        /// </summary>
        /// <param name="source">The record to map from</param>
        /// <returns>A new client object with the mapped values from <paramref name="source"/>.</returns>
        TClientModel MapFromServer(TServerModel source);
    }
}