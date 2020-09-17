using System.Text;
using Newtonsoft.Json;
using Nexus.Link.Libraries.Core.Json;
using Nexus.Link.Libraries.Core.Storage.Model;

namespace Nexus.Link.Libraries.Core.Storage.Logic
{
    /// <summary>
    /// The headers of a request
    /// </summary>
    public class StorableAsByteArray<TData, TId> : StorableByteArray<TId>, IStorableAsByteArray<TData, TId>
    {
        /// <summary>
        /// This property is stored serialized as a byte array
        /// </summary>
        public virtual TData Data
        {
            get
            {
                if (typeof(TData) == typeof(byte[])) return (TData)(object)ByteArray;
                var jsonString = Encoding.UTF8.GetString(ByteArray);
                return JsonHelper.SafeDeserializeObject<TData>(jsonString);
            }
            set
            {
                byte[] valueAsBytes;
                if (typeof(TData) == typeof(byte[]))
                {
                    valueAsBytes = (byte[]) (object) value;
                }
                else
                {
                    var jsonString = JsonConvert.SerializeObject(value);
                    valueAsBytes = Encoding.UTF8.GetBytes(jsonString);
                }
                ByteArray = valueAsBytes;
            }
        }
    }
}
