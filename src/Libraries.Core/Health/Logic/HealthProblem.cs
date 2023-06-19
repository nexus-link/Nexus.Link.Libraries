using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Newtonsoft.Json;
using Nexus.Link.Libraries.Core.Logging;
using Nexus.Link.Libraries.Core.MultiTenant.Model;

namespace Nexus.Link.Libraries.Core.Health.Logic;

public class HealthProblem
{
    public HealthProblem(string id, string resource, string title, Tenant tenant = null)
    {
        Id = id;
        Resource = resource;
        Title = title;
        Tenant = tenant;
        StartedAt = DateTimeOffset.UtcNow;
        LatestAt = StartedAt;
        MessageCounters = new Dictionary<string, int>();
    }

    [JsonProperty]
    public string Id { get; }
    [JsonProperty]
    public string Resource { get; }
    [JsonProperty]
    public string Title { get; }

    /// <summary>
    /// true for Warning. Otherwise regarded as Error.
    /// </summary>
    public bool Warning { get; set; }
    [JsonProperty]
    public Tenant Tenant { get; }
    [JsonProperty]
    public DateTimeOffset StartedAt { get; }
    public DateTimeOffset LatestAt { get; internal set; }

    [JsonProperty]
    public Dictionary<string, int> MessageCounters { get; }

    /// <summary>
    /// Adds the error message and count how many times it has occurred
    /// </summary>
    public void AddMessage(string message)
    {
        lock (MessageCounters)
        {
            if (!MessageCounters.TryGetValue(message, out var count))
            {
                MessageCounters.Add(message, count = 0);
            }
            count++;
            MessageCounters[message] = count;
        }
    }

    public string GetHealthMessage() => $"{Title} (started at {StartedAt.ToLogString()}, latest at {LatestAt.ToLogString()})";
}