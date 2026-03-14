namespace ExaminationSystem.Application.InfraInterfaces;

/// <summary>
/// Provides caching operations.
/// </summary>
public interface ICachingService
{
    /// <summary>
    /// Retrieves a cached value asynchronously.
    /// </summary>
    /// <param name="key">The cache key.</param>
    /// <param name="tenantId">Optional tenant ID to append as a postfix to the key.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The cached value if found, otherwise null.</returns>
    Task<string?> GetAsync(string key, int? tenantId = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a value to the cache asynchronously.
    /// </summary>
    /// <param name="key">The cache key.</param>
    /// <param name="value">The value to cache.</param>
    /// <param name="timeToLive">Optional time-to-live for the cached value.</param>
    /// <param name="tenantId">Optional tenant ID to append as a postfix to the key.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task AddAsync(string key, string value, TimeSpan? timeToLive, int? tenantId = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes a value from the cache asynchronously.
    /// </summary>
    /// <param name="key">The cache key.</param>
    /// <param name="tenantId">Optional tenant ID to append as a postfix to the key.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task RemoveAsync(string key, int? tenantId = null, CancellationToken cancellationToken = default);
}
