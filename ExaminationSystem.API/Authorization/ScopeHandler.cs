using ExaminationSystem.Application.Common;
using Microsoft.AspNetCore.Authorization;

namespace ExaminationSystem.API.Authorization;

/// <summary>
/// Handler that checks whether the current user's JWT contains the required scope claim.
/// </summary>
public class ScopeHandler : AuthorizationHandler<ScopeRequirement>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, ScopeRequirement requirement)
    {
        var scopes = context.User.FindFirst(CustomClaimTypes.Scope)?.Value?.Split(' ');

        if (scopes?.Contains(requirement.Scope) == true)
            context.Succeed(requirement);

        return Task.CompletedTask;
    }
}
