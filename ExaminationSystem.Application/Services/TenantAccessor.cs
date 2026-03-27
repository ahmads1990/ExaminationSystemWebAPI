using ExaminationSystem.Application.Interfaces;

namespace ExaminationSystem.Application.Services;

/// <summary>
/// <see cref="ITenantAccessor"/> implementation using <see cref="AsyncLocal{T}"/>.
/// Registered as a singleton — the AsyncLocal handles per-async-flow isolation.
/// </summary>
public class TenantAccessor : ITenantAccessor
{
    private static readonly AsyncLocal<int?> _currentTenantId = new();

    /// <inheritdoc />
    public int? TenantId => _currentTenantId.Value;

    /// <inheritdoc />
    public void SetTenantId(int tenantId) => _currentTenantId.Value = tenantId;
}
