using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Error.Logic;
using Nexus.Link.Libraries.Core.Storage.Logic;
using Nexus.Link.Libraries.Core.Storage.Model;
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
        }

        /// <inheritdoc />
        public async Task<TId> CreateAsync(TModel item, CancellationToken token = default(CancellationToken))
        {
            var storedItem = Encrypt(item);
            return await _service.CreateAsync(storedItem, token);
        }

        /// <inheritdoc />
        public async Task<TModel> CreateAndReturnAsync(TModel item, CancellationToken token = default(CancellationToken))
        {
            var storedItem = Encrypt(item);
            storedItem = await _service.CreateAndReturnAsync(storedItem, token);
            return Decrypt(storedItem);
        }

        /// <inheritdoc />
        public async Task CreateWithSpecifiedIdAsync(TId id, TModel item, CancellationToken token = default(CancellationToken))
        {
            var storedItem = Encrypt(item);
            await _service.CreateWithSpecifiedIdAsync(id, storedItem, token);
        }

        /// <inheritdoc />
        public async Task<TModel> CreateWithSpecifiedIdAndReturnAsync(TId id, TModel item, CancellationToken token = default(CancellationToken))
        {
            var storedItem = Encrypt(item);
            storedItem = await _service.CreateWithSpecifiedIdAndReturnAsync(id, storedItem, token);
            return Decrypt(storedItem);
        }

        /// <inheritdoc />
        public async Task<TModel> ReadAsync(TId id, CancellationToken token = default(CancellationToken))
        {
            var storedItem = await _service.ReadAsync(id, token);
            return Decrypt(storedItem);
        }

        /// <inheritdoc />
        public async Task<PageEnvelope<TModel>> ReadAllWithPagingAsync(int offset, int? limit = null, CancellationToken token = default(CancellationToken))
        {
            var page = await _service.ReadAllWithPagingAsync(offset, limit, token);
            return new PageEnvelope<TModel>(page.PageInfo, page.Data.Select(Decrypt));

        }

        /// <inheritdoc />
        public async Task<PageEnvelope<TModel>> SearchAsync(SearchDetails<TModel> details, int offset, int? limit = null,
            CancellationToken cancellationToken = default(CancellationToken))
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
        public Task<TModel> SearchFirstAsync(SearchDetails<TModel> details, CancellationToken cancellationToken = default(CancellationToken))
        {
            throw new System.NotImplementedException();
        }

        /// <inheritdoc />
        public Task<TModel> FindUniqueAsync(SearchDetails<TModel> details, CancellationToken cancellationToken = default(CancellationToken))
        {
            throw new System.NotImplementedException();
        }

        /// <inheritdoc />
        public async Task<IEnumerable<TModel>> ReadAllAsync(int limit = int.MaxValue, CancellationToken token = default(CancellationToken))
        {
            var storedItems = await _service.ReadAllAsync(limit, token);
            return storedItems.Select(Decrypt);
        }

        /// <inheritdoc />
        public async Task UpdateAsync(TId id, TModel item, CancellationToken token = default(CancellationToken))
        {
            var storedItem = Encrypt(item);
            await _service.UpdateAsync(id, storedItem, token);
        }

        /// <inheritdoc />
        public async Task<TModel> UpdateAndReturnAsync(TId id, TModel item, CancellationToken token = default(CancellationToken))
        {
            var storedItem = Encrypt(item);
            storedItem = await _service.UpdateAndReturnAsync(id, storedItem, token);
            return Decrypt(storedItem);
        }

        /// <inheritdoc />
        public Task DeleteAsync(TId id, CancellationToken token = default(CancellationToken))
        {
            return _service.DeleteAsync(id, token);
        }

        /// <inheritdoc />
        public Task DeleteAllAsync(CancellationToken token = default(CancellationToken))
        {
            return _service.DeleteAllAsync(token);
        }

        /// <inheritdoc />
        public Task<Lock<TId>> ClaimLockAsync(TId id, CancellationToken token = default(CancellationToken))
        {
            return _service.ClaimLockAsync(id, token);
        }

        /// <inheritdoc />
        public Task ReleaseLockAsync(TId id, TId lockId, CancellationToken token = default(CancellationToken))
        {
            return _service.ReleaseLockAsync(id, lockId, token);
        }

        /// <inheritdoc />
        public Task<Lock<TId>> ClaimDistributedLockAsync(TId id, CancellationToken token = default(CancellationToken))
        {
            return _service.ClaimDistributedLockAsync(id, token);
        }

        /// <inheritdoc />
        public Task ReleaseDistributedLockAsync(TId id, TId lockId, CancellationToken token = default(CancellationToken))
        {
            return _service.ReleaseDistributedLockAsync(id, lockId, token);
        }

        /// <inheritdoc />
        public Task ClaimTransactionLockAsync(TId id, CancellationToken token = default(CancellationToken))
        {
            return _service.ClaimTransactionLockAsync(id, token);
        }
    }
}