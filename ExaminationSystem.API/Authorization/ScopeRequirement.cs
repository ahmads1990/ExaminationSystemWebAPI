using Microsoft.AspNetCore.Authorization;

namespace ExaminationSystem.API.Authorization;

/// <summary>
/// Requirement that checks for a specific scope claim in the JWT token.
/// </summary>
public class ScopeRequirement : IAuthorizationRequirement
{
    public string Scope { get; }
    public ScopeRequirement(string scope) => Scope = scope;
}
