using MapsterMapper;
using Microsoft.AspNetCore.Mvc;

namespace ExaminationSystem.API.Controllers;

[ApiController]
[Route("api/[controller]/[action]")]
[Produces("application/json")]
public class BaseController : ControllerBase
{
    protected readonly IMapper _mapper;

    public BaseController(IMapper mapper)
    {
        _mapper = mapper;
    }
}
