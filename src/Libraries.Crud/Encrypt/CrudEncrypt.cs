using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Error.Logic;
using Nexus.Link.Libraries.Core.Storage.Logic;
using Nexus.Link.Libraries.Core.Storage.Model;
using Nexus.Link.Libraries.Crud.Helpers;
using Nexus.Link.Libraries.Crud.Interfaces;
using Nexus.Link.Libraries.Crud.Model;
using Nexus.Link.Libraries.Crud.PassThrough;

namespace Nexus.Link.Libraries.Crud.Encrypt
{
    /// <inheritdoc cref="EncryptBase{TModel,TId}" />
    public class CrudEncrypt<TModel, TId> :
        EncryptBase<TModel, TId>,
        ICrud<TModel, TId>
    {
        private readonly ICrud<StorableAsByteArray<TModel, TId>, TId> _service;
        private readonly CrudConvenience<TModel, TModel, TId> _convenience;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="service"></param>
        /// <param name="symmetricEncryptionKey"></param>
        public CrudEncrypt(ICrudable<StorableAsByteArray<TModel, TId>, TId> service, byte[] symmetricEncryptionKey)
            : base(symmetricEncryptionKey)
        {
            InternalContract.RequireNotNull(service, nameof(service));
            InternalContract.RequireNotNull(symmetricEncryptionKey, nameof(symmetricEncryptionKey));
            _service = new CrudPassThrough<StorableAsByteArray<TModel, TId>, TId>(service);
            _convenience = new CrudConvenience<TModel, TModel, TId>(this);
        }

        /// <inheritdoc />
        public async Task<TId> CreateAsync(TModel item, CancellationToken cancellationToken  = default)
        {
            var storedItem = Encrypt(item);
            return await _service.CreateAsync(storedItem, cancellationToken );
        }

        /// <inheritdoc />
        public async Task<TModel> CreateAndReturnAsync(TModel item, CancellationToken cancellationToken  = default)
        {
            var storedItem = Encrypt(item);
            storedItem = await _service.CreateAndReturnAsync(storedItem, cancellationToken );
            return Decrypt(storedItem);
        }

        /// <inheritdoc />
        public async Task CreateWithSpecifiedIdAsync(TId id, TModel item, CancellationToken cancellationToken  = default)
        {
            var storedItem = Encrypt(item);
            await _service.CreateWithSpecifiedIdAsync(id, storedItem, cancellationToken );
        }

        /// <inheritdoc />
        public async Task<TModel> CreateWithSpecifiedIdAndReturnAsync(TId id, TModel item, CancellationToken cancellationToken  = default)
        {
            var storedItem = Encrypt(item);
            storedItem = await _service.CreateWithSpecifiedIdAndReturnAsync(id, storedItem, cancellationToken );
            return Decrypt(storedItem);
        }

        /// <inheritdoc />
        public async Task<TModel> ReadAsync(TId id, CancellationToken cancellationToken  = default)
        {
            var storedItem = await _service.ReadAsync(id, cancellationToken );
            return Decrypt(storedItem);
        }

        /// <inheritdoc />
        public async Task<PageEnvelope<TModel>> ReadAllWithPagingAsync(int offset, int? limit = null, CancellationToken cancellationToken  = default)
        {
            var page = await _service.ReadAllWithPagingAsync(offset, limit, cancellationToken );
            return new PageEnvelope<TModel>(page.PageInfo, page.Data.Select(Decrypt));

        }

        /// <inheritdoc />
        public Task<PageEnvelope<TModel>> SearchAsync(SearchDetails<TModel> details, int offset, int? limit = null,
            CancellationToken cancellationToken = default)
        {
            throw new FulcrumNotImplementedException();
            //if (!(_service is ISearch<TModel, TId> searcher))
            //{
            //    throw new FulcrumContractException(
            //        $"The service {_service.GetType().FullName} must implement {nameof(ISearch<TModel, TId>)}.");
            //}

            //var page = await searcher.SearchAsync(details, offset, limit, cancellationToken);
            //return new PageEnvelope<TModel>(page.PageInfo, page.Data.Select(Decrypt));
        }

        /// <inheritdoc />
        public Task<TModel> FindUniqueAsync(SearchDetails<TModel> details, CancellationToken cancellationToken = default)
        {
            throw new System.NotImplementedException();
        }

        /// <inheritdoc />
        public async Task<IEnumerable<TModel>> ReadAllAsync(int limit = int.MaxValue, CancellationToken cancellationToken  = default)
        {
            var storedItems = await _service.ReadAllAsync(limit, cancellationToken );
            return storedItems.Select(Decrypt);
        }

        /// <inheritdoc />
        public async Task UpdateAsync(TId id, TModel item, CancellationToken cancellationToken  = default)
        {
            var storedItem = Encrypt(item);
            await _service.UpdateAsync(id, storedItem, cancellationToken );
        }

        /// <inheritdoc />
        public async Task<TModel> UpdateAndReturnAsync(TId id, TModel item, CancellationToken cancellationToken  = default)
        {
            var storedItem = Encrypt(item);
            storedItem = await _service.UpdateAndReturnAsync(id, storedItem, cancellationToken );
            return Decrypt(storedItem);
        }

        /// <inheritdoc />
        public Task DeleteAsync(TId id, CancellationToken cancellationToken  = default)
        {
            return _service.DeleteAsync(id, cancellationToken );
        }

        /// <inheritdoc />
        public Task DeleteAllAsync(CancellationToken cancellationToken  = default)
        {
            return _service.DeleteAllAsync(cancellationToken );
        }

        /// <inheritdoc />
        public Task<Lock<TId>> ClaimLockAsync(TId id, CancellationToken cancellationToken  = default)
        {
            return _service.ClaimLockAsync(id, cancellationToken );
        }

        /// <inheritdoc />
        public Task ReleaseLockAsync(TId id, TId lockId, CancellationToken cancellationToken  = default)
        {
            return _service.ReleaseLockAsync(id, lockId, cancellationToken );
        }

        /// <inheritdoc />
        public Task<Lock<TId>> ClaimDistributedLockAsync(TId id, TimeSpan? lockTimeSpan = null, TId currentLockId = default,
            CancellationToken cancellationToken = default)
        {
            return _service.ClaimDistributedLockAsync(id, lockTimeSpan, currentLockId, cancellationToken );
        }

        /// <inheritdoc />
        public Task ReleaseDistributedLockAsync(TId id, TId lockId, CancellationToken cancellationToken  = default)
        {
            return _service.ReleaseDistributedLockAsync(id, lockId, cancellationToken );
        }

        /// <inheritdoc />
        public Task ClaimTransactionLockAsync(TId id, CancellationToken cancellationToken  = default)
        {
            return _service.ClaimTransactionLockAsync(id, cancellationToken );
        }

        /// <inheritdoc />
        public Task<TModel> ClaimTransactionLockAndReadAsync(TId id, CancellationToken cancellationToken  = default)
        {
            return _convenience.ClaimTransactionLockAndReadAsync(id, cancellationToken );
        }
    }
}