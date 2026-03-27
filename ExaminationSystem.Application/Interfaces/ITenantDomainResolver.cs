namespace ExaminationSystem.Application.Interfaces;

/// <summary>
/// Resolves a tenant identifier from a request domain (hostname).
/// </summary>
public interface ITenantDomainResolver
{
    /// <summary>
    /// Looks up the tenant ID for the given domain. Returns null if no matching tenant is found.
    /// Results are cached for performance.
    /// </summary>
    Task<int?> ResolveTenantIdByDomainAsync(string domain, CancellationToken cancellationToken = default);
}
