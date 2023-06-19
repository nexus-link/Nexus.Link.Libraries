using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Nexus.Link.Libraries.Core.Application;
using Nexus.Link.Libraries.Core.Health.Model;
using Nexus.Link.Libraries.Core.MultiTenant.Model;

namespace Nexus.Link.Libraries.Core.Health.Logic
{
    /// <summary>
    /// Knows the logic behind aggregating health of many resources.
    /// </summary>
    public class ResourceHealthAggregator2
    {
        /// <summary>
        /// The current Tenant
        /// </summary>
        public Tenant Tenant { get; }

        private readonly Model.Health _health;
        private int _warnings;
        private int _errors;
        private string _lastErrorMessage;
        private string _lastWarningMessage;

        /// <summary>
        /// The signature for a resource health method.
        /// </summary>
        public delegate Task<HealthInfo> GetResourceHealthDelegate(Tenant tenant, CancellationToken cancellationToken = default);

        /// <summary>
        /// Create ResourceHealthAggregator2 with an <see cref="Tenant"/> and the name of the service
        /// </summary>
        /// <param name="tenant">The tenant that we should focus on.</param>
        /// <param name="serviceName">The name of the resource.</param>
        public ResourceHealthAggregator2(Tenant tenant, string serviceName)
        {
            Tenant = tenant;
            _health = new Model.Health(serviceName);

            MaybeAddHealthTrackerProblems();
        }

        private void MaybeAddHealthTrackerProblems()
        {
            var problems = Tenant == null
                ? FulcrumApplication.Setup.HealthTracker.GetAllHealthProblems()
                : FulcrumApplication.Setup.HealthTracker.GetHealthProblems(Tenant)
                    .Concat(FulcrumApplication.Setup.HealthTracker.GetHealthProblems())
                    .ToList();

            foreach (var problem in problems)
            {
                AddHealthResponse(new()
                {
                    Status = problem.Warning ? HealthInfo.StatusEnum.Warning : HealthInfo.StatusEnum.Error,
                    Resource = problem.Resource,
                    Message = problem.GetHealthMessage(),
                    SerializedData = JsonConvert.SerializeObject(problem)
                });
            }
        }

        /// <summary>
        /// Check the health of a specific resource and aggregate it to the complete health state.
        /// </summary>
        /// <param name="resourceName">The name to use for the resource</param>
        /// <param name="resource">A resource that we want to get the health for and add it to the aggregated health.</param>
        /// <param name="cancellationToken"></param>
        public async Task AddResourceHealthAsync(string resourceName, IResourceHealth2 resource, CancellationToken cancellationToken = default)
        {
            await AddResourceHealthAsync(resourceName, resource.GetResourceHealth2Async, cancellationToken);
        }

        /// <summary>
        /// Call a health check delegate and aggregate the answer to the complete health state.
        /// </summary>
        /// <param name="resourceName">The name to use for the resource</param>
        /// <param name="healthDelegate">A method that returns a health, that we will add to the aggregated health.</param>
        /// <param name="cancellationToken"></param>
        public async Task AddResourceHealthAsync(string resourceName, GetResourceHealthDelegate healthDelegate, CancellationToken cancellationToken = default)
        {
            if (Tenant == null) return;

            HealthInfo response;
            try
            {
                response = await healthDelegate(Tenant, cancellationToken);
                //Check this?
                if (string.IsNullOrWhiteSpace(response.Resource)) response.Resource = resourceName;
            }
            catch (Exception e)
            {
                response = new HealthInfo(resourceName)
                {
                    Status = HealthInfo.StatusEnum.Error,
                    Message = e.Message
                };
            }
            AddHealthResponse(response);
        }

        /// <summary>
        /// Add a health response and aggregate it to the complete health state.
        /// </summary>
        /// <param name="healthInfo"></param>
        public void AddHealthResponse(HealthInfo healthInfo)
        {
            _health.Resources.Add(healthInfo);
            if (healthInfo.Status == HealthInfo.StatusEnum.Warning)
            {
                _warnings++;
                _lastWarningMessage = healthInfo.Message;
            }
            if (healthInfo.Status == HealthInfo.StatusEnum.Error)
            {
                _errors++;
                _lastErrorMessage = healthInfo.Message;
            }
        }

        /// <summary>
        /// Get the aggregated health.
        /// </summary>
        /// <returns></returns>
        public Model.Health GetAggregatedHealthResponse()
        {
            if (_errors > 0)
            {
                _health.Status = HealthInfo.StatusEnum.Error;
                _health.Message = _errors == 1 ? _lastErrorMessage : $"Found {_errors} errors and {_warnings} warnings.";
            }
            else if (_warnings > 0)
            {
                _health.Status = HealthInfo.StatusEnum.Warning;
                _health.Message = _errors == 1 ? _lastWarningMessage : $"Found {_warnings} warnings.";
            }
            else
            {
                _health.Status = HealthInfo.StatusEnum.Ok;
                _health.Message = "OK";
            }
            return _health;
        }
    }
}
