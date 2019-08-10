#if NETCOREAPP
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Nexus.Link.Libraries.Core.Assert;

namespace Nexus.Link.Libraries.Web.AspNet.Authorize
{
    /// <summary>
    /// This requirement verifies that the caller has a specific, mandatory role.
    /// </summary>
    /// <remarks>Use <see cref="SetMandatoryRole"/> to set which role that should be mandatory for your app.</remarks>
    public class MandatoryRoleRequirement : AuthorizationHandler<MandatoryRoleRequirement>, IAuthorizationRequirement
    {
        private static string _mandatoryRole;

        /// <summary>
        /// Use this once to set a mandatory role for all services that has the attribute <see cref="MandatoryRoleRequirement"/>.
        /// </summary>
        /// <param name="roleName">The role to use for this application, e.g. 'business-api-caller'.</param>
        public static void SetMandatoryRole(string roleName)
        {
            InternalContract.Require(_mandatoryRole == null, 
                $"The mandatory role has already been set ({_mandatoryRole}). You are not allowed to change it.");
            _mandatoryRole = roleName;
        }

        /// <inheritdoc />
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, MandatoryRoleRequirement requirement)
        {
            if (requirement.HasMandatoryRole(context)) context.Succeed(requirement);
            else context.Fail();
            return Task.CompletedTask;
        }

        private bool HasMandatoryRole(AuthorizationHandlerContext context)
        {
            if (string.IsNullOrWhiteSpace(_mandatoryRole)) return true;
            return context?.User != null && context.User.IsInRole(_mandatoryRole);
        }
    }
}
#endif