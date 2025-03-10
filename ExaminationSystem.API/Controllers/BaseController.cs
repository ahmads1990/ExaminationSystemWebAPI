using Microsoft.AspNetCore.Mvc;

namespace ExaminationSystem.API.Controllers;

[ApiController]
[Route("api/[controller]/[action]")]
[Produces("application/json")]
public class BaseController : ControllerBase
{
    public BaseController()
    {
    }
}
