using System;
using System.Collections.Generic;
using System.Linq;
using Nexus.Link.Libraries.Core.MultiTenant.Model;

namespace Nexus.Link.Libraries.Core.Health.Logic;

public interface IHealthTracker
{
    void AddHealthProblemMessage(string id, string resource, string title, string message, Tenant tenant = null);
    void ResetHealthProblem(string id, Tenant tenant = null);
    IReadOnlyList<HealthProblem> GetHealthProblems(Tenant tenant = null);
    IReadOnlyList<HealthProblem> GetAllHealthProblems();
    void ResetAllHealthProblems();
}

public class HealthTracker : IHealthTracker
{
    private static readonly Dictionary<string, HealthProblem> States = new();

    private static string CreateKey(string id, Tenant tenant = null)
    {
        return $"{id}/{tenant?.Organization}/{tenant?.Environment}";
    }

    public HealthProblem GetHealthProblem(string id, Tenant tenant = null)
    {
        lock (States)
        {
            var key = CreateKey(id, tenant);
            return States.TryGetValue(key, out var state) ? state : null;
        }
    }

    public void SetHealthProblem(HealthProblem healthProblem)
    {
        var key = CreateKey(healthProblem.Id, healthProblem.Tenant);
        
        healthProblem.LatestAt = DateTimeOffset.UtcNow;

        lock (States)
        {
            States[key] = healthProblem;
        }
    }


    public void ResetHealthProblem(string id, Tenant tenant = null)
    {
        var key = CreateKey(id, tenant);
        lock (States)
        {
            States.Remove(key);
        }
    }

    /// <inheritdoc />
    public void AddHealthProblemMessage(string id, string resource, string title, string message, Tenant tenant = null)
    {
        lock (States)
        {
            var state = GetHealthProblem(id, tenant) ?? new HealthProblem(id, resource, title, tenant);
            state.AddMessage(message);
            SetHealthProblem(state);
        }
    }

    /// <inheritdoc />
    public void AddHealthProblemMessage(string id, string resource, string title, Exception e, Tenant tenant = null)
    {
        AddHealthProblemMessage(id, resource, title, $"{e.GetType().FullName}: {e.Message}", tenant);
    }

    public void ResetAllHealthProblems()
    {
        lock (States)
        {
            States.Clear();
        }
    }

    public IReadOnlyList<HealthProblem> GetHealthProblems(Tenant tenant = null)
    {
        lock (States)
        {
            return tenant == null
                ? States.Values.Where(_ => _.Tenant == null).ToList()
                : States.Values.Where(_ => tenant.Equals(_.Tenant)).ToList();
        }
    }

    public IReadOnlyList<HealthProblem> GetAllHealthProblems()
    {
        lock (States)
        {
            return States.Values.ToList();
        }
    }
}