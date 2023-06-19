using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Nexus.Link.Libraries.Core.MultiTenant.Model;

namespace Nexus.Link.Libraries.Core.Health.Logic;

public interface IHealthTracker
{
    ProblemState GetProblemState(string id, Tenant tenant = null);
    void SetProblemState(ProblemState state);
    void ResetProblemState(string id, Tenant tenant = null);
    IReadOnlyList<ProblemState> GetProblems(Tenant tenant = null);
    IReadOnlyList<ProblemState> GetAllProblems();
    void ResetAllProblemStates();
}

public class HealthTracker : IHealthTracker
{
    private static readonly Dictionary<string, ProblemState> States = new();

    private static string CreateKey(string id, Tenant tenant = null)
    {
        return $"{id}/{tenant?.Organization}/{tenant?.Environment}";
    }

    public ProblemState GetProblemState(string id, Tenant tenant = null)
    {
        lock (States)
        {
            var key = CreateKey(id, tenant);
            return States.TryGetValue(key, out var state) ? state : null;
        }
    }

    public void SetProblemState(ProblemState state)
    {
        var key = CreateKey(state.Id, state.Tenant);
        
        state.LatestAt = DateTimeOffset.UtcNow;

        lock (States)
        {
            States[key] = state;
        }
    }

    public void ResetProblemState(string id, Tenant tenant = null)
    {
        var key = CreateKey(id, tenant);
        lock (States)
        {
            States.Remove(key);
        }
    }

    public void ResetAllProblemStates()
    {
        lock (States)
        {
            States.Clear();
        }
    }

    public IReadOnlyList<ProblemState> GetProblems(Tenant tenant = null)
    {
        lock (States)
        {
            return tenant == null
                ? States.Values.Where(_ => _.Tenant == null).ToList()
                : States.Values.Where(_ => tenant.Equals(_.Tenant)).ToList();
        }
    }

    public IReadOnlyList<ProblemState> GetAllProblems()
    {
        lock (States)
        {
            return States.Values.ToList();
        }
    }
}

public class ProblemState
{
    public ProblemState(string id, string resource, string title, Tenant tenant = null)
    {
        Id = id;
        Resource = resource;
        Title = title;
        Tenant = tenant;
        StartedAt = DateTimeOffset.UtcNow;
        LatestAt = StartedAt;
        ErrorMessages = new ConcurrentDictionary<string, int>();
    }

    [JsonProperty]
    public string Id { get; }

    /// <summary>
    /// true for Warning. Otherwise regarded as Error.
    /// </summary>
    [JsonProperty]
    public string Resource { get; }
    [JsonProperty]
    public string Title { get; }
    public bool Warning { get; set; }
    [JsonProperty]
    public Tenant Tenant { get; }
    [JsonProperty]
    public DateTimeOffset StartedAt { get; }
    public DateTimeOffset LatestAt { get; internal set; }

    [JsonProperty]
    public ConcurrentDictionary<string, int> ErrorMessages { get; }

    /// <summary>
    /// Adds the error message and count how many times it has occurred
    /// </summary>
    public void AddError(string message)
    {
        lock (ErrorMessages)
        {
            if (!ErrorMessages.TryGetValue(message, out var count))
            {
                ErrorMessages.TryAdd(message, count = 0);
            }
            count++;
            ErrorMessages[message] = count;
        }
    }

    /// <summary>
    /// Adds an error message constructed from <paramref name="e"/> and a count how many times it has occurred
    /// </summary>
    public void AddError(Exception e)
    {
        AddError($"{e.GetType().FullName}: {e.Message}");
    }
}