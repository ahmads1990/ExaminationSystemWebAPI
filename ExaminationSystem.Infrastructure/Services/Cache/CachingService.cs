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
    public async Task<string?> GetAsync(string key, CancellationToken cancellationToken = default)
    {
        return await _distributedCache.GetStringAsync(key, cancellationToken);
    }

    /// <inheritdoc />
    public async Task AddAsync(string key, string value, TimeSpan? timeToLive, CancellationToken cancellationToken = default)
    {
        if (timeToLive.HasValue)
        {
            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = timeToLive
            };
            await _distributedCache.SetStringAsync(key, value, options, cancellationToken);
        }

        await _distributedCache.SetStringAsync(key, value, cancellationToken);
    }

    /// <inheritdoc />
    public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        await _distributedCache.RemoveAsync(key, cancellationToken);
    }

    #endregion
}
