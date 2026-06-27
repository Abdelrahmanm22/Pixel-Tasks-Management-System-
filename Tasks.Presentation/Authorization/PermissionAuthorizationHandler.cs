using Microsoft.AspNetCore.Authorization;
using Tasks.Domain.Authorization;

namespace Tasks.Presentation.Authorization
{
    public class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
    {
        protected override Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            PermissionRequirement requirement)
        {
            if (context.User.HasClaim(Permissions.ClaimType, requirement.Permission))
                context.Succeed(requirement);

            return Task.CompletedTask;
        }
    }
}
