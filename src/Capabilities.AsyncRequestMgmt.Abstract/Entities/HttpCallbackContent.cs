namespace Nexus.Link.Capabilities.AsyncRequestMgmt.Abstract.Entities
{
    /// <summary>
    /// The object that is sent as content for a callback.
    /// </summary>
    public class HttpCallbackContent
    {
        /// <summary>
        /// The final HttpResponse
        /// </summary>
        public HttpResponse Response { get; set; }

        /// <summary>
        /// The request id for the original request.
        /// </summary>
        public string RequestId { get; set; }

        /// <summary>
        /// The context that the client submitted for the original request.
        /// </summary>
        public string Context { get; set; }
    }
}