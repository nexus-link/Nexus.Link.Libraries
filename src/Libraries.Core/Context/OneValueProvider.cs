using Nexus.Link.Libraries.Core.Assert;

namespace Nexus.Link.Libraries.Core.Context
{
    /// <summary>
    /// Convenience for choosing the right <see cref="IValueProvider"/>.
    /// </summary>
    public class OneValueProvider<T>
    {
        private readonly IValueProvider _valueProvider;

        /// <summary>
        /// The key to the context value that this class represents.
        /// </summary>
        public string Key { get; }

        /// <summary>
        /// Constructor
        /// </summary>
        public OneValueProvider(IValueProvider valueProvider, string key)
        {
            _valueProvider = valueProvider;
            InternalContract.RequireNotNullOrWhiteSpace(key, nameof(key));
            Key = key;
        }

        /// <summary>
        /// Get the context value
        /// </summary>
        public T GetValue()
        {
            FulcrumAssert.IsNotNullOrWhiteSpace(Key);
            return _valueProvider.GetValue<T>(Key);
        }

        /// <summary>
        /// Set the context value
        /// </summary>
        public void SetValue(T data)
        {
            FulcrumAssert.IsNotNullOrWhiteSpace(Key);
            _valueProvider.SetValue(Key, data);
        }
    }
}
