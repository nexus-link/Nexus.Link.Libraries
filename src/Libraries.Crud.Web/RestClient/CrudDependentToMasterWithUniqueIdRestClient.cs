using System;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Storage.Model;
using Nexus.Link.Libraries.Crud.Interfaces;
using Nexus.Link.Libraries.Web.RestClientHelper;

namespace Nexus.Link.Libraries.Crud.Web.RestClient
{
    /// <inheritdoc cref="CrudDependentToMasterWithUniqueIdRestClient{TModelCreate,TModel,TId, TDependentId}" />
    public class CrudDependentToMasterWithUniqueIdRestClient<TModel, TId, TDependentId> :
        CrudDependentToMasterWithUniqueIdRestClient<TModel, TModel, TId, TDependentId>,
        ICrudDependentToMasterWithUniqueId<TModel, TId, TDependentId>
        where TModel : IUniquelyIdentifiable<TId>
    {

        /// <summary></summary>
        /// <param name="parentName">The name of the sub path that is the parent of the children. (Singular noun)</param>
        /// <param name="childrenName">The name of the sub path that are the children. (Plural noun)</param>
        /// <param name="httpSender">How to actually send HTTP requests.</param>
        public CrudDependentToMasterWithUniqueIdRestClient(string parentName, string childrenName, IHttpSender httpSender)
            : base(parentName, childrenName, httpSender)
        {
        }
    }

    /// <inheritdoc cref="Nexus.Link.Libraries.Crud.Web.RestClient" />
    public class CrudDependentToMasterWithUniqueIdRestClient<TManyModelCreate, TManyModel, TId, TDependentId> : 
        CrudDependentToMasterRestClient<TManyModelCreate, TManyModel, TId, TDependentId>, 
        ICrudDependentToMasterWithUniqueId<TManyModelCreate, TManyModel, TId, TDependentId> 
        where TManyModel : TManyModelCreate 
        where TManyModelCreate : IUniquelyIdentifiable<TId>
    {
        /// <summary></summary>
        /// <param name="parentName">The name of the sub path that is the parent of the children. (Singular noun)</param>
        /// <param name="childrenName">The name of the sub path that are the children. (Plural noun)</param>
        /// <param name="httpSender">How to actually send HTTP requests.</param>
        public CrudDependentToMasterWithUniqueIdRestClient(string parentName, string childrenName, IHttpSender httpSender)
            : base(parentName, childrenName, httpSender)
        {
        }

        /// <inheritdoc />
        public Task<TManyModel> ReadAsync(TId id, CancellationToken token = default)
        {
            InternalContract.RequireNotDefaultValue(id, nameof(id));
            return GetAsync<TManyModel>($"{ChildrenName}/{id}", cancellationToken: token);
        }

        /// <inheritdoc />
        public Task UpdateAsync(TId id, TManyModel item, CancellationToken token = default)
        {
            InternalContract.RequireNotDefaultValue(id, nameof(id));
            InternalContract.RequireNotNull(item, nameof(item));
            InternalContract.RequireValidated(item, nameof(item));
            if (item is IUniquelyIdentifiable<TId> uniquelyIdentifiable)
            {
                InternalContract.RequireAreEqual(id, uniquelyIdentifiable.Id, $"{nameof(item)}.{nameof(uniquelyIdentifiable.Id)}");
            }
            return PutNoResponseContentAsync($"{ChildrenName}/{id}", item, null, token);
        }

        /// <inheritdoc />
        public Task<TManyModel> UpdateAndReturnAsync(TId id, TManyModel item, CancellationToken token = default)
        {
            InternalContract.RequireNotDefaultValue(id, nameof(id));
            InternalContract.RequireNotNull(item, nameof(item));
            InternalContract.RequireValidated(item, nameof(item));
            if (item is IUniquelyIdentifiable<TId> uniquelyIdentifiable)
            {
                InternalContract.RequireAreEqual(id, uniquelyIdentifiable.Id, $"{nameof(item)}.{nameof(uniquelyIdentifiable.Id)}");
            }
            return PutAndReturnUpdatedObjectAsync($"{ChildrenName}/{id}", item, null, token);
        }

        /// <inheritdoc />
        public Task DeleteAsync(TId id, CancellationToken token = default)
        {
            InternalContract.RequireNotDefaultValue(id, nameof(id));
            return DeleteAsync($"{ChildrenName}/{id}", cancellationToken: token);
        }
    }
}
