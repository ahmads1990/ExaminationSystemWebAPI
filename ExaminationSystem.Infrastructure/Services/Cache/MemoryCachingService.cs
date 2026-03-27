using ExaminationSystem.Application.InfraInterfaces;
using Microsoft.Extensions.Caching.Memory;

namespace ExaminationSystem.Infrastructure.Services.Cache;

/// <summary>
/// <see cref="ICachingService"/> implementation backed by <see cref="IMemoryCache"/>.
/// Used as a drop-in replacement for <see cref="CachingService"/> (Redis) when running
/// in demo/dev environments without a Redis instance.
/// </summary>
public class MemoryCachingService : ICachingService
{
    #region Fields

    private readonly IMemoryCache _memoryCache;

    #endregion

    #region Constructors

    public MemoryCachingService(IMemoryCache memoryCache)
    {
        _memoryCache = memoryCache;
    }

    #endregion

    #region Public Methods

    /// <inheritdoc />
    public Task<string?> GetAsync(string key, int? tenantId = null, CancellationToken cancellationToken = default)
    {
        _memoryCache.TryGetValue(BuildKey(key, tenantId), out string? value);
        return Task.FromResult(value);
    }

    /// <inheritdoc />
    public Task AddAsync(string key, string value, TimeSpan? timeToLive, int? tenantId = null, CancellationToken cancellationToken = default)
    {
        var resolvedKey = BuildKey(key, tenantId);

        if (timeToLive.HasValue)
        {
            var options = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = timeToLive
            };
            _memoryCache.Set(resolvedKey, value, options);
        }
        else
        {
            _memoryCache.Set(resolvedKey, value);
        }

        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task RemoveAsync(string key, int? tenantId = null, CancellationToken cancellationToken = default)
    {
        _memoryCache.Remove(BuildKey(key, tenantId));
        return Task.CompletedTask;
    }

    #endregion

    #region Private Methods

    private static string BuildKey(string key, int? tenantId)
        => tenantId.HasValue ? $"{key}:tenant:{tenantId.Value}" : key;

    #endregion
}
