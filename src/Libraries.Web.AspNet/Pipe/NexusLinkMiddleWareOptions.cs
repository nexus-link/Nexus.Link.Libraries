using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Logging;
using Nexus.Link.Libraries.Core.Platform.Configurations;

namespace Nexus.Link.Libraries.Web.AspNet.Pipe
{
    public class NexusLinkMiddleWareOptions : INexusLinkMiddleWareOptions
    {

        /// <summary>
        /// The way that many XLENT Link services has prefixed tenants in their path. Not recommended. <see cref="ApiVersionTenantPrefix"/> for the recommended prefix.
        /// </summary>
        public const string LegacyVersionPrefix = "/v[^/]+";

        /// <summary>
        /// A slightly safer way than <see cref="LegacyVersionPrefix"/>. Not recommended. <see cref="ApiVersionTenantPrefix"/> for the recommended prefix.
        /// </summary>
        public const string LegacyApiVersionPrefix = "api/v[^/]+";

        /// <summary>
        /// The current recommended prefix for tenant in path
        /// </summary>
        public const string ApiVersionTenantPrefix = "api/v[^/]+/Tenant";

        /// <inheritdoc/>
        public virtual bool UseFeatureBatchLog { get; set; }

        /// <inheritdoc/>
        public virtual LogSeverityLevel BatchLogThreshold { get; set;} = LogSeverityLevel.Error;

        /// <inheritdoc/>
        public virtual bool BatchLogReleaseRecordsAsLateAsPossible { get; set;}

        /// <inheritdoc />
        public virtual bool UseFeatureLogRequestAndResponse { get; set; }

        /// <inheritdoc />
        public virtual bool UseFeatureConvertExceptionToHttpResponse { get; set; }

        /// <inheritdoc />
        public virtual bool UseFeatureSaveClientTenantToContext { get; set; }

        /// <inheritdoc />
        public virtual string SaveClientTenantPrefix { get; set; }

        /// <inheritdoc />
        public virtual bool UseFeatureSaveTenantConfigurationToContext { get; set; }

        /// <inheritdoc />
        public virtual ILeverServiceConfiguration SaveTenantConfigurationServiceConfiguration { get; set; }

        /// <inheritdoc />
        public virtual bool UseFeatureSaveCorrelationIdToContext { get; set; }

        /// <inheritdoc />
        public virtual bool UseFeatureSaveNexusTestContextToContext { get; set; }

        /// <inheritdoc />
        public virtual void Validate(string errorLocation, string propertyPath = "")
        {
            if (UseFeatureSaveClientTenantToContext)
            {
                FulcrumValidate.IsNotNullOrWhiteSpace(SaveClientTenantPrefix, nameof(SaveClientTenantPrefix), errorLocation);
            }

            if (UseFeatureSaveTenantConfigurationToContext)
            {
                FulcrumValidate.IsTrue(UseFeatureSaveClientTenantToContext, errorLocation,
                    $"When you use the feature flag {nameof(UseFeatureSaveTenantConfigurationToContext)}, then you also need {nameof(UseFeatureSaveClientTenantToContext)}");
                FulcrumValidate.IsNotNull(SaveTenantConfigurationServiceConfiguration, nameof(SaveTenantConfigurationServiceConfiguration), errorLocation);
            }
        }
    }
}