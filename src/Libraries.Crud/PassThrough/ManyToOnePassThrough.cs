﻿using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Libraries.Crud.Interfaces;
using Nexus.Link.Libraries.Core.Storage.Model;
using Nexus.Link.Libraries.Crud.Helpers;
using Nexus.Link.Libraries.Crud.Model;

namespace Nexus.Link.Libraries.Crud.PassThrough
{
    /// <inheritdoc cref="ManyToOnePassThrough{TModelCreate,TModel,TId}" />
    public class ManyToOnePassThrough<TModel, TId> : 
        ManyToOnePassThrough<TModel, TModel, TId>,
        ICrudManyToOne<TModel, TId>
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="service">The crud class to pass things down to.</param>
        public ManyToOnePassThrough(ICrudable<TModel, TId> service)
            : base(service)
        {
        }
    }

    /// <inheritdoc cref="ICrudManyToOne{TModelCreate,TModel,TId}" />
    public class ManyToOnePassThrough<TModelCreate, TModel, TId> : 
        CrudPassThrough<TModelCreate, TModel, TId>, 
        ICrudManyToOne<TModelCreate, TModel, TId> 
        where TModel : TModelCreate
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="service">The crud class to pass things down to.</param>
        public ManyToOnePassThrough(ICrudable<TModel, TId> service)
            : base(service)
        {
        }

        /// <inheritdoc />
        public virtual Task<PageEnvelope<TModel>> ReadChildrenWithPagingAsync(TId parentId, int offset, int? limit = null,
            CancellationToken cancellationToken  = default)
        {
            var implementation = CrudHelper.GetImplementationOrThrow<IReadChildrenWithPaging<TModel, TId>>(Service);
            return implementation.ReadChildrenWithPagingAsync(parentId, offset, limit, cancellationToken );
        }

        /// <inheritdoc />
        public virtual Task<IEnumerable<TModel>> ReadChildrenAsync(TId parentId, int limit = Int32.MaxValue, CancellationToken cancellationToken  = default)
        {
            var implementation = CrudHelper.GetImplementationOrThrow<IReadChildren<TModel, TId>>(Service);
            return implementation.ReadChildrenAsync(parentId, limit, cancellationToken );
        }

        /// <inheritdoc />
        public virtual Task DeleteChildrenAsync(TId parentId, CancellationToken cancellationToken  = default)
        {
            var implementation = CrudHelper.GetImplementationOrThrow<IDeleteChildren<TId>>(Service);
            return implementation.DeleteChildrenAsync(parentId, cancellationToken );
        }

        /// <inheritdoc />
        public Task<PageEnvelope<TModel>> SearchChildrenAsync(TId parentId, SearchDetails<TModel> details, int offset, int? limit = null,
            CancellationToken cancellationToken = default)
        {
            var implementation = CrudHelper.GetImplementationOrThrow<ISearchChildren<TModel, TId>>(Service);
            return implementation.SearchChildrenAsync(parentId, details, offset, limit, cancellationToken);
        }

        /// <inheritdoc />
        public Task<TModel> FindUniqueChildAsync(TId parentId, SearchDetails<TModel> details,
            CancellationToken cancellationToken = default)
        {
            var implementation = CrudHelper.GetImplementationOrThrow<ISearchChildren<TModel, TId>>(Service);
            return implementation.FindUniqueChildAsync(parentId, details, cancellationToken);
        }

        /// <inheritdoc />
        public Task<TId> CreateChildAsync(TId parentId, TModelCreate item, CancellationToken cancellationToken  = default)
        {
            var implementation = CrudHelper.GetImplementationOrThrow<ICreateChild<TModelCreate, TModel, TId>>(Service);
            return implementation.CreateChildAsync(parentId, item, cancellationToken );
        }

        /// <inheritdoc />
        public Task<TModel> CreateChildAndReturnAsync(TId parentId, TModelCreate item, CancellationToken cancellationToken  = default)
        {
            var implementation = CrudHelper.GetImplementationOrThrow<ICreateChildAndReturn<TModelCreate, TModel, TId>>(Service);
            return implementation.CreateChildAndReturnAsync(parentId, item, cancellationToken );
        }

        /// <inheritdoc />
        public Task CreateChildWithSpecifiedIdAsync(TId parentId, TId childId, TModelCreate item, CancellationToken cancellationToken  = default)
        {
            var implementation = CrudHelper.GetImplementationOrThrow<ICreateChildWithSpecifiedId<TModelCreate, TModel, TId>>(Service);
            return implementation.CreateChildWithSpecifiedIdAsync(parentId, childId, item, cancellationToken );
        }

        /// <inheritdoc />
        public Task<TModel> CreateChildWithSpecifiedIdAndReturnAsync(TId parentId, TId childId, TModelCreate item, CancellationToken cancellationToken  = default)
        {
            var implementation = CrudHelper.GetImplementationOrThrow<ICreateChildWithSpecifiedIdAndReturn<TModelCreate, TModel, TId>>(Service);
            return implementation.CreateChildWithSpecifiedIdAndReturnAsync(parentId, childId, item, cancellationToken );
        }
    }
}
