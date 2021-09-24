namespace Nexus.Link.Libraries.Web.Pipe
{
    public class Constants
    {
        /// <summary>
        /// Standard correlation id header
        /// </summary>
        public static string FulcrumCorrelationIdHeaderName = "X-Correlation-ID";       
        
        /// <summary>
        /// The header for the Async Manager execution id
        /// </summary>
        public static string ExecutionIdHeaderName = "X-Nexus-Async-ExecutionId";

        /// <summary>
        /// Standard correlation id header
        /// </summary>
        public static string PreferHeaderName = "Prefer";

        /// <summary>
        /// Standard correlation id header
        /// </summary>
        public static string PreferRespondAsyncHeaderValue = "respond-async";

        /// <summary>
        /// Header to indicate that a request is done in test mode
        /// </summary>
        public static string NexusTestContextHeaderName = "X-nexus-test-context";

        /// <summary>
        /// For propagating end user authentication token
        /// </summary>
        public const string NexusUserAuthorizationHeaderName = "Nexus-User-Authorization";

        /// <summary>
        /// For propagating a translated user id
        /// </summary>
        /// <remarks>
        /// Setup by the Business API and used in capability prooviders
        /// </remarks>
        public const string NexusTranslatedUserIdHeaderName = "Nexus-Translated-User-Id";

        /// <summary>
        /// Key for setting up user authorization token on async local context
        /// </summary>
        public const string NexusUserAuthorizationKeyName = "NexusUserAuthorization";

        /// <summary>
        /// Key for setting up translated user id on async local context
        /// </summary>
        public const string TranslatedUserIdKey = "NexusTranslatedUserId";
    }
}
