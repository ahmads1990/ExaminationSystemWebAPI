using ExaminationSystem.Application.Interfaces;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace ExaminationSystem.Application.Services;

public class CurrentUserService : ICurrentUserService
{
    #region Fields

    private readonly IHttpContextAccessor _httpContextAccessor;

    #endregion

    #region Constructors

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    #endregion

    #region Public Methods

    /// <inheritdoc />
    public int? UserId
    {
        get
        {
            var userIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(userIdClaim, out var userId) ? userId : null;
        }
    }

    /// <inheritdoc />
    public bool IsAuthenticated
    {
        get
        {
            return _httpContextAccessor.HttpContext?
                       .User?
                       .Identity?
                       .IsAuthenticated ?? false;
        }
    }

    #endregion
}
