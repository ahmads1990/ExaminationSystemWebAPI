using ExaminationSystem.Application.DTOs.Tenants;
using ExaminationSystem.Application.InfraInterfaces;
using ExaminationSystem.Application.Interfaces;
using ExaminationSystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace ExaminationSystem.Application.Services;

public class TenantService : ITenantService
{
    #region Constants

    private const string TenantsCacheKey = "tenants:all_active";
    private static readonly TimeSpan CacheDuration = TimeSpan.FromHours(1);

    #endregion

    #region Fields

    private readonly DbContext _dbContext;
    private readonly ICachingService _cachingService;
    private readonly ILogger<TenantService> _logger;

    #endregion

    #region Constructors

    public TenantService(DbContext dbContext, ICachingService cachingService, ILogger<TenantService> logger)
    {
        _dbContext = dbContext;
        _cachingService = cachingService;
        _logger = logger;
    }

    #endregion

    #region Public Methods

    /// <inheritdoc />
    public async Task<List<TenantLookupDto>> GetAllTenantsAsync(CancellationToken cancellationToken = default)
    {
        // Try cache first
        var cached = await _cachingService.GetAsync(TenantsCacheKey, cancellationToken: cancellationToken);
        if (!string.IsNullOrEmpty(cached))
        {
            return JsonSerializer.Deserialize<List<TenantLookupDto>>(cached) ?? new List<TenantLookupDto>();
        }

        // Query from DB
        var tenants = await _dbContext.Set<Tenant>()
            .Where(t => t.IsActive)
            .Select(t => new TenantLookupDto
            {
                ID = t.ID,
                Name = t.Name
            })
            .ToListAsync(cancellationToken);

        // Cache the result
        var serialized = JsonSerializer.Serialize(tenants);
        await _cachingService.AddAsync(TenantsCacheKey, serialized, CacheDuration, cancellationToken: cancellationToken);

        _logger.LogInformation("Cached {Count} active tenants", tenants.Count);

        return tenants;
    }

    #endregion
}
