using System;
using System.Collections.Generic;
using System.Linq;
using Nexus.Link.Libraries.Core.EntityAttributes;

namespace Nexus.Link.Libraries.Web.Error.Logic;

/// <summary>
/// The server has accepted the request and will execute it in the background. 
/// </summary>
/// <remarks>
/// This class has three flavors, so inherit from this to make more explicit variants:
/// Call me again in a while: Set <see cref="TryAgainAfterMinimumTimeSpan"/>
/// Don't call me, I will fix this: Don't set anything.
/// Call me when one of a number of requests has been completed: Set <see cref="WaitingForRequestIds"/>
/// </remarks>
public abstract class RequestPostponedException : Exception
{
    protected RequestPostponedException(TimeSpan? tryAgainAfterMinimumTimeSpan = null)
    {
        TryAgainAfterMinimumTimeSpan = tryAgainAfterMinimumTimeSpan;
    }

    /// <summary>
    /// Constructor
    /// </summary>
    protected RequestPostponedException(IEnumerable<string> waitingForRequestIds)
    {
        AddWaitingForIds(waitingForRequestIds);
    }

    /// <summary>
    /// Constructor
    /// </summary>
    protected RequestPostponedException(params string[] waitingForRequestIds)
    {
        AddWaitingForIds(waitingForRequestIds);
    }

    /// <summary>
    /// True means that there was a problem, so the caller should try to call us again.
    /// </summary>
    [Obsolete($"Obsolete since 2023-09-08. Please set {nameof(TryAgainAfterMinimumTimeSpan)} to mark for try again. You might set it to TimeSpan.Zero for ASAP retry.")]
    public bool TryAgain { get; set; }

    /// <summary>
    /// Set this property to give the minimum expected time before a retry is made.
    /// </summary>
    public TimeSpan? TryAgainAfterMinimumTimeSpan { get; set; }

    /// <summary>
    /// If there are any other requests that we are waiting for, they will be listed here.
    /// </summary>
    [Validation.NotNull]
    public List<string> WaitingForRequestIds { get; set; } = new List<string>();

    /// <summary>
    /// This value can be used instead of normal authentication to continue a postponed execution.
    /// </summary>
    public string ReentryAuthentication { get; set; }

    /// <summary>
    /// Add requests that we are waiting for.
    /// </summary>
    /// <param name="waitingForRequestIds">Requests that we are waiting for</param>
    /// <returns></returns>
    public RequestPostponedException AddWaitingForIds(IEnumerable<string> waitingForRequestIds)
    {
        if (waitingForRequestIds == null) return this;
        WaitingForRequestIds.AddRange(waitingForRequestIds.Where(ri => ri != null));
        return this;
    }

    /// <summary>
    /// Add requests that we are waiting for.
    /// </summary>
    /// <param name="waitingForRequestIds">Requests that we are waiting for</param>
    /// <returns></returns>
    public RequestPostponedException AddWaitingForIds(params string[] waitingForRequestIds)
    {
        if (waitingForRequestIds == null) return this;
        WaitingForRequestIds.AddRange(waitingForRequestIds.Where(ri => ri != null));
        return this;
    }
}

/// <summary>
/// To be used internally as a concrete implementation
/// </summary>
internal class InternalRequestPostponedException : RequestPostponedException
{
    public InternalRequestPostponedException(IEnumerable<string> postponeInfoWaitingForRequestIds) 
        : base(postponeInfoWaitingForRequestIds)
    {
    }
}