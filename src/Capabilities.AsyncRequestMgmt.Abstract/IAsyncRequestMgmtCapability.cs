using Nexus.Link.Capabilities.AsyncRequestMgmt.Abstract.Services;

namespace Nexus.Link.Capabilities.AsyncRequestMgmt.Abstract
{
    /// <summary>
    /// Access to the Request.
    /// </summary>
    public interface IAsyncRequestMgmtCapability
    {
        /// <summary>
        /// The <see cref="IRequestService"/>.
        /// </summary>
        IRequestService Request { get; }

        /// <summary>
        /// The <see cref="IRequestResponseService"/>.
        /// </summary>
        IRequestResponseService RequestResponse { get; }

        /// <summary>
        /// The <see cref="IExecutionService"/>.
        /// </summary>
        IExecutionService Execution { get; }

        /// <summary>
        /// The <see cref="IExecutionResponseService"/>.
        /// </summary>
        IExecutionResponseService ExecutionResponse { get; }
    }
}
