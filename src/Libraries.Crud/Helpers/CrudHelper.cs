using System;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Error.Logic;
using Nexus.Link.Libraries.Crud.Interfaces;
using Nexus.Link.Libraries.Crud.Mappers;

namespace Nexus.Link.Libraries.Crud.Helpers
{
    /// <summary>
    /// Helper methods for Storage
    /// </summary>
    public static class CrudHelper
    {
        /// <summary>
        /// Create a new Id of type <see cref="string"/> or type <see cref="Guid"/>.
        /// </summary>
        /// <typeparam name="TId"></typeparam>
        /// <returns></returns>
        public static TId CreateNewId<TId>()
        {
            var id = default(TId);
            if (typeof(TId) == typeof(Guid))
            {
                // ReSharper disable once SuspiciousTypeConversion.Global
                id = (dynamic)Guid.NewGuid();
            }
            else if (typeof(TId) == typeof(string))
            {
                // ReSharper disable once SuspiciousTypeConversion.Global
                id = (dynamic)Guid.NewGuid().ToString();
            }
            else
            {
                FulcrumAssert.Fail(null,
                    $"{nameof(CreateNewId)} can handle Guid and string as type for Id, but it can't handle {typeof(TId)}.");
            }
            return id;
        }

        /// <summary>
        /// If <paramref name="service"/> doesn't implement <typeparamref name="T"/>, an exception is thrown.
        /// </summary>
        /// <param name="service">The service that must implement <typeparamref name="T"/>.</param>
        /// <typeparam name="T">The type that <paramref name="service"/> must implement.</typeparam>
        /// <returns></returns>
        /// <exception cref="FulcrumNotImplementedException">Thrown if <paramref name="service"/> doesn't implement <typeparamref name="T"/>.</exception>
        public static T GetImplementationOrThrow<T>(ICrudable service) where T : ICrudable
        {
            InternalContract.RequireNotNull(service, nameof(service));
            if (service is T implemented) return implemented;
            throw new FulcrumNotImplementedException($"The service {service.GetType()} does not implement {typeof(T).Name}");
        }

        /// <summary>
        /// If <paramref name="service"/> doesn't implement <typeparamref name="T"/>, an exception is thrown.
        /// </summary>
        /// <param name="service">The service that must implement <typeparamref name="T"/>.</param>
        /// <typeparam name="T">The type that <paramref name="service"/> must implement.</typeparam>
        /// <returns></returns>
        /// <exception cref="FulcrumNotImplementedException">Thrown if <paramref name="service"/> doesn't implement <typeparamref name="T"/>.</exception>
        public static T GetImplementationOrThrow<T>(IMappable service) where T : IMappable
        {
            if (service is T implemented) return implemented;
            throw new FulcrumNotImplementedException($"The service {service.GetType()} does not implement {typeof(T).Name}");
        }
    }
}
