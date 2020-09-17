using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Crud.Helpers;
using Nexus.Link.Libraries.Crud.Mappers;

namespace Nexus.Link.Libraries.Crud.PassThrough
{
    /// <inheritdoc cref="MapperPassThrough{TModelCreate,TModel,TId}" />
    public class MapperPassThrough<TModel, TServerModel> :
        MapperPassThrough<TModel, TModel, TServerModel>
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="service">The crud class to pass things down to.</param>
        public MapperPassThrough(IMappable<TModel, TServerModel> service)
            : base(service)
        {
        }
    }

    /// <summary>
    /// Verify that the service given in the constructor has the neccessary map implementations.
    /// </summary>
    public class MapperPassThrough<TModelCreate, TModel, TServerModel> :
        IMapper<TModelCreate, TModel, TServerModel> where TModel : TModelCreate
    {
        /// <summary>
        /// The service that should do the actual mapping
        /// </summary>
        protected IMappable<TModel, TServerModel> Service { get; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="service">The crud class to pass things down to.</param>
        public MapperPassThrough(IMappable<TModel, TServerModel> service)
        {
            InternalContract.RequireNotNull(service, nameof(service));
            Service = service;
        }

        /// <inheritdoc />
        public virtual TServerModel MapToServer(TModelCreate source)
        {
            var implementation = CrudHelper.GetImplementationOrThrow<ICreateMapper<TModelCreate, TModel, TServerModel>>(Service);
            return implementation.MapToServer(source);
        }

        /// <inheritdoc />
        public virtual TServerModel MapToServer(TModel source)
        {
            var implementation = CrudHelper.GetImplementationOrThrow<IUpdateMapper<TModel, TServerModel>>(Service);
            return implementation.MapToServer(source);
        }

        /// <inheritdoc />
        public virtual TModel MapFromServer(TServerModel source)
        {
            var implementation = CrudHelper.GetImplementationOrThrow<IReadMapper<TModel, TServerModel>>(Service);
            return implementation.MapFromServer(source);
        }
    }
}
