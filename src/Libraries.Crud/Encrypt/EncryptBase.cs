using Nexus.Link.Libraries.Core.Security;
using Nexus.Link.Libraries.Core.Storage.Model;

namespace Nexus.Link.Libraries.Crud.Encrypt
{
    /// <inheritdoc />
    public class EncryptBase <TModel, TId>
    {
        private readonly SymmetricCrypto _symmetricCrypto;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="symmetricEncryptionKey"></param>
        protected EncryptBase(byte[] symmetricEncryptionKey)
        {
            _symmetricCrypto = new SymmetricCrypto(symmetricEncryptionKey);
        }

        #region Protected
        /// <summary>
        /// Encrypt an <paramref name="item"/> into a StorableAsByteArray.
        /// </summary>
        protected Core.Storage.Logic.StorableAsByteArray<TModel, TId> Encrypt(TModel item)
        {
            var storedItem = new Core.Storage.Logic.StorableAsByteArray<TModel, TId>
            {
                Data = item
            };
            storedItem.ByteArray = _symmetricCrypto.Encrypt(storedItem.ByteArray);
            return storedItem;
        }

        /// <summary>
        /// Returns a <paramref name="storedItem"/> decryped into an item.
        /// </summary>
        protected TModel Decrypt(IStorableAsByteArray<TModel, TId> storedItem)
        {
            storedItem.ByteArray = _symmetricCrypto.Decrypt(storedItem.ByteArray);
            return storedItem.Data;
        }
        #endregion
    }
}