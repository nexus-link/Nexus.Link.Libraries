using System;

namespace Nexus.Link.Libraries.Crud.Mappers
{
    /// <inheritdoc cref="IMapper{TClientModel,TServerModel}" />
    /// <typeparam name="TClientModel">The model the client uses when updating items.</typeparam>
    /// <typeparam name="TServerModel">The model that the server uses. </typeparam>
    [Obsolete("We no longer recommend to use this mapping technique. Obsolete warning since 2020-09-23, error since 2021-06-09.", true)]
    public interface IMapper<TClientModel, TServerModel> : IMapper<TClientModel, TClientModel, TServerModel>
    {
    }

    /// <summary>
    /// Methods for mapping data between the client and server models.
    /// </summary>
    /// <typeparam name="TClientModelCreate">The model that the client uses when creating items.</typeparam>
    /// <typeparam name="TClientModel">The model the client uses when updating items.</typeparam>
    /// <typeparam name="TServerModel">The model that the server uses. </typeparam>
    [Obsolete("We no longer recommend to use this mapping technique. Obsolete warning since 2020-09-23, error since 2021-06-09.", true)]
    public interface IMapper<in TClientModelCreate, TClientModel, TServerModel> : 
        ICreateMapper<TClientModelCreate, TClientModel, TServerModel>, 
        IUpdateMapper<TClientModel, TServerModel>, 
        IReadMapper<TClientModel, TServerModel>
    {
    }
}