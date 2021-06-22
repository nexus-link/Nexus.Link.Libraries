using System;

namespace Nexus.Link.Libraries.Web.Pipe
{
    public class Constants
    {
        /// <summary>
        /// Standard correlation id header
        /// </summary>
        public static string FulcrumCorrelationIdHeaderName = "X-Correlation-ID";

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
    }
}
