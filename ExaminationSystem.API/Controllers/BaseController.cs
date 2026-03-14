using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;

namespace ExaminationSystem.API.Controllers;

/// <summary>
/// Provides a base class for all API controllers, offering common properties and utility methods.
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Produces("application/json")]
public class BaseController : ControllerBase
{
    #region Constructors

    /// <summary>
    /// Initializes a new instance of the <see cref="BaseController"/> class.
    /// </summary>
    public BaseController()
    {
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Gets the unique identifier of the currently authenticated user.
    /// Returns <c>null</c> if the user is not authenticated or the ID claim is missing.
    /// </summary>
    public int? CurrentUserId
    {
        get
        {
            var userIdClaim = User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(userIdClaim, out var userId) ? userId : null;
        }
    }

    /// <summary>
    /// Gets a value indicating whether the current request is authenticated.
    /// </summary>
    public bool IsAuthenticated => User.Identity?.IsAuthenticated ?? false;

    #endregion
}
