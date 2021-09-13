using System.Collections.Generic;
using System.Net.Http;

namespace Nexus.Link.Libraries.Web.Pipe.Outbound
{
    /// <summary>
    /// A factory class to create delegating handlers for outgoing HTTP requests.
    /// </summary>
    public static class OutboundPipeFactory
    {

        /// <summary>
        /// Creates handlers to deal with Fulcrum specifics around making HTTP requests.
        /// </summary>
        /// <seealso cref="ThrowFulcrumExceptionOnFail"/>
        /// <seealso cref="AddCorrelationId"/>
        /// <seealso cref="LogRequestAndResponse"/>
        /// <returns>A list of recommended handlers.</returns>
        public static DelegatingHandler[] CreateDelegatingHandlers()
        {
            return CreateDelegatingHandlers(true);
        }

        /// <summary>
        /// Creates handlers to deal with Fulcrum specifics around making HTTP requests, but without any logging
        /// </summary>
        /// <seealso cref="ThrowFulcrumExceptionOnFail"/>
        /// <seealso cref="AddCorrelationId"/>
        /// <returns>A list of recommended handlers.</returns>
        public static DelegatingHandler[] CreateDelegatingHandlersWithoutLogging()
        {
            return CreateDelegatingHandlers(false);
        }

        /// <summary>
        /// Creates handlers specialized for a Business API service
        /// </summary>
        public static DelegatingHandler[] CreateDelegatingHandlersForBusinessApi(bool withLogging)
        {
            return CreateDelegatingHandlers(withLogging, true);
        }


        private static DelegatingHandler[] CreateDelegatingHandlers(bool withLogging, bool asBusinessApi = false)
        {
            var handlers = new List<DelegatingHandler>
            {
                new ThrowFulcrumExceptionOnFail(),
                new AddCorrelationId(),
            };
            
            if (asBusinessApi)
            {
                handlers.Add(new AddUserAuthorization());
                handlers.Add(new AddTranslatedUserId());
            }
            
            if (withLogging)
            {
                handlers.Add(new LogRequestAndResponse());
            }
            
            handlers.Add(new PropagateNexusTestHeader());
            
            return handlers.ToArray();
        }
    }
}
