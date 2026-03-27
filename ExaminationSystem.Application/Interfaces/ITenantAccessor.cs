namespace ExaminationSystem.Application.Interfaces;

/// <summary>
/// Provides ambient access to the current tenant's identifier.
/// Backed by <see cref="System.Threading.AsyncLocal{T}"/> so it flows across async/await
/// boundaries and works in background jobs (Hangfire) without requiring an HTTP context.
/// </summary>
public interface ITenantAccessor
{
    /// <summary>
    /// Gets the current tenant identifier, or null if no tenant has been resolved yet.
    /// </summary>
    int? TenantId { get; }

    /// <summary>
    /// Sets the tenant identifier for the current async execution flow.
    /// Called by middleware (HTTP) or job filters (Hangfire).
    /// </summary>
    void SetTenantId(int tenantId);
}
