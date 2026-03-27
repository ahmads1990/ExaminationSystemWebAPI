namespace ExaminationSystem.Application.Common;

/// <summary>
/// Defines the action to take when a request arrives from an unknown (unmapped) domain.
/// Configured via appsettings under "Tenancy:UnknownDomainAction".
/// </summary>
public enum UnknownDomainAction
{
    /// <summary>
    /// Return a 400 Bad Request response. Recommended for production.
    /// </summary>
    RejectRequest = 0,

    /// <summary>
    /// Fall back to the default tenant (TenantId = 1). Useful during development.
    /// </summary>
    UseDefaultTenant = 1
}
