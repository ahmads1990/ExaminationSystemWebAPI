using ExaminationSystem.Application.DTOs.Tenants;
using ExaminationSystem.Application.Interfaces;
using ExaminationSystem.API.Models.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ExaminationSystem.API.Controllers;

/// <summary>
/// Provides tenant lookup operations for frontend dropdowns.
/// </summary>
public class TenantsController : BaseController
{
    private readonly ITenantService _tenantService;

    public TenantsController(ITenantService tenantService)
    {
        _tenantService = tenantService;
    }

    /// <summary>
    /// Gets all active tenants for dropdown lists (cached).
    /// </summary>
    /// <returns>A list of active tenant names and IDs.</returns>
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(SuccessResponse<List<TenantLookupDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var tenants = await _tenantService.GetAllTenantsAsync(cancellationToken);
        return Ok(new SuccessResponse<List<TenantLookupDto>>(tenants));
    }
}
