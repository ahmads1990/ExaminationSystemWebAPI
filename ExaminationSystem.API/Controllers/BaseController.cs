using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ExaminationSystem.API.Controllers;

[ApiController]
[Route("api/[controller]/[action]")]
[Produces("application/json")]
public class BaseController : ControllerBase
{
    public int? CurrentUserId
    {
        get
        {
            var userIdClaim = User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(userIdClaim, out var userId) ? userId : null;
        }
    }

    public bool IsAuthenticated => User.Identity?.IsAuthenticated ?? false;

    public BaseController()
    {
    }
}
