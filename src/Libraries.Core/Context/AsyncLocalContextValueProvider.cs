using Nexus.Link.Libraries.Core.Assert;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

namespace Nexus.Link.Libraries.Core.Context
{
    /// <summary>
    /// Stores values in the execution context which is unaffected by asynchronous code that switches threads or context. 
    /// </summary>
    /// <remarks>Updating values in a thread will not affect the value in parent/sibling threads</remarks>
    public class AsyncLocalContextValueProvider : IContextValueProvider
    {
        private static readonly ConcurrentDictionary<string, AsyncLocal<object>> Dictionary = new ConcurrentDictionary<string, AsyncLocal<object>>();

        /// <inheritdoc />
        public T GetValue<T>(string key)
        {
            InternalContract.RequireNotNullOrWhiteSpace(key, nameof(key));
            if (!Dictionary.TryGetValue(key, out AsyncLocal<object> asyncLocalObject)) return default;
            var o = asyncLocalObject.Value;
            if (o == null) return default;
            return (T)o;
        }

        /// <inheritdoc />
        public void SetValue<T>(string key, T data)
        {
            InternalContract.RequireNotNullOrWhiteSpace(key, nameof(key));
            var addedAsyncLocal = new AsyncLocal<object> { Value = data };
            if (!Dictionary.TryAdd(key, addedAsyncLocal))
            {
                if (Dictionary.TryGetValue(key, out var existingAsyncLocal))
                {
                    existingAsyncLocal.Value = data;
                }
                else
                {
                    FulcrumAssert.Fail($"Should really not happen as keys are only added, never removed. Key: '{key}'");
                }
            }
        }

        /// <inheritdoc />
        public Guid ContextId => GetValue<Guid>("ContextId");

        /// <inheritdoc />
        public void Reset()
        {
            foreach (var entry in Dictionary)
            {
                SetValue(entry.Key, (object)null);
            }
            SetValue("ContextId", Guid.NewGuid());
        }

        /// <inheritdoc />
        public void RestoreContext(IDictionary<string, object> dictionary)
        {
            if (dictionary == null)
            {
                Reset();
                return;
            }

            Reset();
            foreach (var entry in dictionary)
            {
                SetValue(entry.Key, entry.Value);
            }
        }

        /// <inheritdoc />
        public IDictionary<string, object> SaveContext()
        {
            if (Dictionary == null) return null;
            var copy = new Dictionary<string, object>();
            foreach (var entry in Dictionary)
            {
                copy.Add(entry.Key, entry.Value.Value);
            }

            return copy;
        }
    }
}
