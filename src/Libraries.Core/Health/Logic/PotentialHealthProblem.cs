using System;
using Nexus.Link.Libraries.Core.Application;
using Nexus.Link.Libraries.Core.MultiTenant.Model;

namespace Nexus.Link.Libraries.Core.Health.Logic;

public class PotentialHealthProblem
{
    public PotentialHealthProblem(string id, string resource, string title)
    {
        Id = id;
        Resource = resource;
        Title = title;
    }
    public string Id { get; }

    public string Resource { get; }

    public string Title { get; }

    public Tenant Tenant { get; set; }

    /// <summary>
    /// true for Warning. Otherwise regarded as Error.
    /// </summary>
    public bool Warning { get; set; }

    public void Fail(string message)
    {
        FulcrumApplication.Setup.HealthTracker.AddHealthProblemMessage(Id, Resource, Title, message, Tenant);
    }

    public void Fail(Exception e)
    {
        FulcrumApplication.Setup.HealthTracker.AddHealthProblemMessage(Id, Resource, Title, $"{e.GetType().FullName}: {e.Message}", Tenant);
    }

    public void Success()
    {
        FulcrumApplication.Setup.HealthTracker.ResetHealthProblem(Id, Tenant);
    }
}