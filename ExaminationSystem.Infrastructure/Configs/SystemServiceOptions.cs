namespace ExaminationSystem.Infrastructure.Configs;

/// <summary>
/// Top-level toggles for optional infrastructure services.
/// Useful when running a demo/dev environment without a Redis or Seq instance available.
/// Bound from appsettings under "SystemServiceOptions".
/// </summary>
public class SystemServiceOptions
{
    /// <summary>
    /// When true, uses in-process <see cref="Microsoft.Extensions.Caching.Memory.IMemoryCache"/>
    /// instead of the distributed Redis cache. All cache operations remain the same — only the
    /// backing store changes.
    /// </summary>
    public bool UseMemoryCache { get; set; } = false;

    /// <summary>
    /// When true, structured logs are written to Seq in addition to the console.
    /// When false, logs fall back to a rolling file sink (logs/log-.txt) instead.
    /// </summary>
    public bool WriteToSeq { get; set; } = true;
}
