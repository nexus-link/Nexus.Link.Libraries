using Newtonsoft.Json.Linq;
using Nexus.Link.Libraries.Core.MultiTenant.Model;
using Nexus.Link.Libraries.Core.Platform.Configurations;

namespace Nexus.Link.Libraries.Web.Tests
{
    public class MockLeverConfiguration : ILeverConfiguration
    {

        /// <inheritdoc />
        public Tenant Tenant { get; }

        /// <summary>
        /// This is where all the configuration values are stored
        /// </summary>
        public JObject JObject { get; internal set; }

        public MockLeverConfiguration(JObject jObject, Tenant tenant = null)
        {
            JObject = jObject;
            Tenant = tenant;
        }

        /// <inheritdoc />
        public T Value<T>(object key)
        {
            return JObject.Value<T>(key);
        }

        /// <inheritdoc />
        public T MandatoryValue<T>(object key)
        {
            var value = Value<T>(key);
            return value;
        }
    }
}