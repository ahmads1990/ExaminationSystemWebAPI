using ExaminationSystem.Application.Common;

namespace ExaminationSystem.Infrastructure.Configs;

/// <summary>
/// Configuration for tenant resolution behavior.
/// Bound from appsettings "Tenancy" section.
/// </summary>
public class TenancyConfig
{
    /// <summary>
    /// Determines what happens when a request arrives from an unmapped domain.
    /// Default: <see cref="UnknownDomainAction.RejectRequest"/>
    /// </summary>
    public UnknownDomainAction UnknownDomainAction { get; set; } = UnknownDomainAction.RejectRequest;

    /// <summary>
    /// The default tenant ID to use when <see cref="UnknownDomainAction"/> is
    /// <see cref="UnknownDomainAction.UseDefaultTenant"/>.
    /// </summary>
    public int DefaultTenantId { get; set; } = 1;

    /// <summary>
    /// Request paths that bypass tenant resolution entirely (e.g. health checks, Hangfire dashboard).
    /// </summary>
    public string[] ExcludedPaths { get; set; } = new[] { "/health", "/hangfire" };
}
