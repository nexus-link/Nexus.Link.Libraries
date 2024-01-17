using System;
using System.Collections.Generic;
using System.Linq;
using Nexus.Link.Libraries.Core.MultiTenant.Model;

namespace Nexus.Link.Libraries.Core.Health.Logic;

public interface IHealthTracker
{
    void AddHealthProblemMessage(string id, string resource, string title, string message, Tenant tenant = null,
        TimeSpan? keepFor = null);
    void ResetHealthProblem(string id, Tenant tenant = null);
    IReadOnlyList<HealthProblem> GetHealthProblems(Tenant tenant = null);
    IReadOnlyList<HealthProblem> GetAllHealthProblems();
    void ResetAllHealthProblems();
}

public class HealthTracker : IHealthTracker
{
    private static readonly Dictionary<string, HealthProblem> HealthProblems = new();

    private static string CreateKey(string id, Tenant tenant = null)
    {
        return $"{id}/{tenant?.Organization}/{tenant?.Environment}";
    }


    public void ResetHealthProblem(string id, Tenant tenant = null)
    {
        var key = CreateKey(id, tenant);
        lock (HealthProblems)
        {
            HealthProblems.Remove(key);
        }
    }

    /// <inheritdoc />
    public void AddHealthProblemMessage(string id, string resource, string title, string message, 
        Tenant tenant = null, TimeSpan? keepFor = null)
    {
        lock (HealthProblems)
        {
            var healthProblem = GetHealthProblem(id, tenant) ?? new HealthProblem(id, resource, title, tenant);
            if (keepFor.HasValue)
            {
                healthProblem.ExpiresAt = DateTimeOffset.UtcNow.Add(keepFor.Value);
            }
            healthProblem.AddMessage(message);
            SetHealthProblem(healthProblem);
        }
    }

    public void ResetAllHealthProblems()
    {
        lock (HealthProblems)
        {
            HealthProblems.Clear();
        }
    }

    public IReadOnlyList<HealthProblem> GetHealthProblems(Tenant tenant = null)
    {
        PurgeHealthProblems();
        lock (HealthProblems)
        {
            return tenant == null
                ? HealthProblems.Values.Where(_ => _.Tenant == null).ToList()
                : HealthProblems.Values.Where(_ => tenant.Equals(_.Tenant)).ToList();
        }
    }

    public IReadOnlyList<HealthProblem> GetAllHealthProblems()
    {
        PurgeHealthProblems();
        lock (HealthProblems)
        {
            return HealthProblems.Values.ToList();
        }
    }

    private void PurgeHealthProblems()
    {
        var now = DateTimeOffset.UtcNow;
        lock (HealthProblems)
        {
            var expiredHealthProblems = HealthProblems.Values.Where(_ => _.HasExpired(now)).ToList();
            foreach (var healthProblem in expiredHealthProblems)
            {
                ResetHealthProblem(healthProblem.Id, healthProblem.Tenant);
            }
        }
    }

    private HealthProblem GetHealthProblem(string id, Tenant tenant = null)
    {
        lock (HealthProblems)
        {
            var key = CreateKey(id, tenant);
            if (!HealthProblems.TryGetValue(key, out var healthProblem)) return null;
            if (!healthProblem.HasExpired()) return healthProblem;
            ResetHealthProblem(healthProblem.Id, healthProblem.Tenant);
            return null;
        }
    }

    private void SetHealthProblem(HealthProblem healthProblem)
    {
        var key = CreateKey(healthProblem.Id, healthProblem.Tenant);

        healthProblem.LatestAt = DateTimeOffset.UtcNow;

        lock (HealthProblems)
        {
            HealthProblems[key] = healthProblem;
        }
    }
}