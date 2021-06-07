using Nexus.Link.Libraries.Core.Application;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Web.AspNet.Pipe.Options;
using Nexus.Link.Libraries.Web.Pipe;

namespace Nexus.Link.Libraries.Web.AspNet.Pipe
{
    public class NexusLinkMiddleWareOptions : IValidatable
    {
        /// <summary>
        /// If one log message in a batch has a severity level equal to or higher than <see name="LogAllThreshold"/>,
        /// then all the logs within that batch will be logged, regardless of the value of
        /// <see cref="ApplicationSetup.LogSeverityLevelThreshold"/>.
        /// </summary>
        public BatchLogOptions BatchLogOptions { get; } = new BatchLogOptions();

        /// <summary>
        /// The prefix before the "/{organization}/{environment}/" part of the path. This is used to pattern match where we would find the organization and environment.
        /// Here are some common patterns: <see cref="SaveClientTenantOptions.LegacyVersionPrefix"/>, <see cref="SaveClientTenantOptions.LegacyApiVersionPrefix"/>,
        /// <see cref="SaveClientTenantOptions.ApiVersionTenantPrefix"/>
        /// </summary>
        public SaveClientTenantOptions SaveClientTenantOptions { get; } = new SaveClientTenantOptions();

        /// <summary>
        /// Log request and response
        /// </summary>
        public LogRequestAndResponseOptions LogRequestAndResponseOptions { get; } = new LogRequestAndResponseOptions();

        /// <summary>
        /// If an API method throws an exception, then this feature will convert it into a regular HTTP response.
        /// </summary>
        public ConvertExceptionToHttpResponseOptions Type { get; } = new ConvertExceptionToHttpResponseOptions();

        /// <summary>
        /// This feature retrieves the tenant configuration and saves it to the <see cref="FulcrumApplication.Context"/>.
        /// </summary>
        public SaveTenantConfigurationOptions SaveTenantConfigurationOptions { get; } = new SaveTenantConfigurationOptions();

        /// <summary>
        /// This feature gets the first found <see cref="Constants.FulcrumCorrelationIdHeaderName"/> header from the request and saves it to the <see cref="FulcrumApplication.Context"/>.
        /// </summary>
        public SaveCorrelationIdOptions SaveCorrelationIdOptions { get; } = new SaveCorrelationIdOptions();


        /// <summary>
        /// This feature reads the <see cref="Constants.NexusTestContextHeaderName"/> header from the request and save it to the execution context.
        /// </summary>
        public SaveNexusTestContextOptions SaveNexusTestContextOptions { get; } = new SaveNexusTestContextOptions();

        /// <inheritdoc />
        public virtual void Validate(string errorLocation, string propertyPath = "")
        {
            // TODO
            //if (UseFeatureSaveClientTenantToContext)
            //{
            //    FulcrumValidate.IsNotNullOrWhiteSpace(SaveClientTenantPrefix, nameof(SaveClientTenantPrefix), errorLocation);
            //}

            //if (UseFeatureSaveTenantConfigurationToContext)
            //{
            //    FulcrumValidate.IsTrue(UseFeatureSaveClientTenantToContext, errorLocation,
            //        $"When you use the feature flag {nameof(UseFeatureSaveTenantConfigurationToContext)}, then you also need {nameof(UseFeatureSaveClientTenantToContext)}");
            //    FulcrumValidate.IsNotNull(SaveTenantConfigurationServiceConfiguration, nameof(SaveTenantConfigurationServiceConfiguration), errorLocation);
            //}
        }
    }
}