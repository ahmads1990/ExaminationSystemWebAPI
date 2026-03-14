using ExaminationSystem.Application.InfraInterfaces;
using Microsoft.Extensions.Caching.Distributed;

namespace ExaminationSystem.Infrastructure.Services;

public class CachingService : ICachingService
{
    #region Fields

    private readonly IDistributedCache _distributedCache;

    #endregion

    #region Constructors

    public CachingService(IDistributedCache distributedCache)
    {
        _distributedCache = distributedCache;
    }

    #endregion

    #region Public Methods

    /// <inheritdoc />
    public async Task<string?> GetAsync(string key, int? tenantId = null, CancellationToken cancellationToken = default)
    {
        return await _distributedCache.GetStringAsync(BuildKey(key, tenantId), cancellationToken);
    }

    /// <inheritdoc />
    public async Task AddAsync(string key, string value, TimeSpan? timeToLive, int? tenantId = null, CancellationToken cancellationToken = default)
    {
        var resolvedKey = BuildKey(key, tenantId);

        if (timeToLive.HasValue)
        {
            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = timeToLive
            };
            await _distributedCache.SetStringAsync(resolvedKey, value, options, cancellationToken);
        }

        await _distributedCache.SetStringAsync(resolvedKey, value, cancellationToken);
    }

    /// <inheritdoc />
    public async Task RemoveAsync(string key, int? tenantId = null, CancellationToken cancellationToken = default)
    {
        await _distributedCache.RemoveAsync(BuildKey(key, tenantId), cancellationToken);
    }

    #endregion

    #region Private Methods

    /// <summary>
    /// Builds the final cache key, appending a tenant postfix if tenantId is provided.
    /// </summary>
    private static string BuildKey(string key, int? tenantId)
        => tenantId.HasValue ? $"{key}:tenant:{tenantId.Value}" : key;

    #endregion
}
