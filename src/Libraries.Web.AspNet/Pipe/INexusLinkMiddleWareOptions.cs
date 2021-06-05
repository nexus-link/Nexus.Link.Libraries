using Nexus.Link.Libraries.Core.Application;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Logging;
using Nexus.Link.Libraries.Core.Platform.Configurations;
using Nexus.Link.Libraries.Web.Pipe;

namespace Nexus.Link.Libraries.Web.AspNet.Pipe
{
    public interface INexusLinkMiddleWareOptions : IValidatable
    {
        /// <summary>
        /// If one log message in a batch has a severity level equal to or higher than <see name="LogAllThreshold"/>,
        /// then all the logs within that batch will be logged, regardless of the value of
        /// <see cref="ApplicationSetup.LogSeverityLevelThreshold"/>.
        /// </summary>
        bool UseFeatureBatchLog { get; }

        /// <summary>
        /// The threshold for logging all messages within a batch.
        /// </summary>
        LogSeverityLevel BatchLogThreshold { get; }

        /// <summary>
        /// True means that the records will be released at the end of the batch.
        /// False means that they will be released as soon as one message hits the threshold and then all messages will be released instantly until the batch ends.
        /// </summary>
        bool BatchLogReleaseRecordsAsLateAsPossible { get; }

        /// <summary>
        /// Log request and response
        /// </summary>
        bool UseFeatureLogRequestAndResponse { get; set; }

        /// <summary>
        /// If an API method throws an exception, then this feature will convert it into a regular HTTP response.
        /// </summary>
        bool UseFeatureConvertExceptionToHttpResponse { get; set; }

        /// <summary>
        /// This feature extracts the client tenant from the URL based on <see cref="SaveClientTenantPrefix"/> and saves it into the <see cref="FulcrumApplication.Context"/>.
        /// </summary>
        bool UseFeatureSaveClientTenantToContext { get; set; }

        /// <summary>
        /// The prefix before the "/{organization}/{environment}/" part of the path. This is used to pattern match where we would find the organization and environment.
        /// Here are some common patterns: <see cref="NexusLinkMiddleWareOptions.LegacyVersionPrefix"/>, <see cref="NexusLinkMiddleWareOptions.LegacyApiVersionPrefix"/>,
        /// <see cref="NexusLinkMiddleWareOptions.ApiVersionTenantPrefix"/>
        /// </summary>
        string SaveClientTenantPrefix { get; set; }
        
        /// <summary>
        /// This feature retrieves the tenant configuration and saves it to the <see cref="FulcrumApplication.Context"/>.
        /// </summary>
        bool UseFeatureSaveTenantConfigurationToContext { get; set; }
        
        /// <summary>
        /// 
        /// </summary>
        ILeverServiceConfiguration SaveTenantConfigurationServiceConfiguration { get; set; }

        /// <summary>
        /// This feature gets the first found <see cref="Constants.FulcrumCorrelationIdHeaderName"/> header from the request and saves it to the <see cref="FulcrumApplication.Context"/>.
        /// </summary>
        bool UseFeatureSaveCorrelationIdToContext { get; set; }

        /// <summary>
        /// This feature reads the <see cref="Constants.NexusTestContextHeaderName"/> header from the request and save it to the execution context.
        /// </summary>
        bool UseFeatureSaveNexusTestContextToContext { get; set; }
    }
}