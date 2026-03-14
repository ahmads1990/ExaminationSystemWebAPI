using ExaminationSystem.Application.DTOs.Tenants;

namespace ExaminationSystem.Application.Interfaces;

/// <summary>
/// Provides tenant lookup operations.
/// </summary>
public interface ITenantService
{
    /// <summary>
    /// Gets all active tenants for lookup dropdowns 
    /// </summary>
    Task<List<TenantLookupDto>> GetAllTenantsAsync(CancellationToken cancellationToken = default);
}
