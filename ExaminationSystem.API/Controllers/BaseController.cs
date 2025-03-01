using MapsterMapper;
using Microsoft.AspNetCore.Mvc;

namespace ExaminationSystem.API.Controllers;

[ApiController]
[Route("[controller]/[action]")]
public class BaseController : ControllerBase
{
    protected readonly IMapper _mapper;

    public BaseController(IMapper mapper)
    {
        _mapper = mapper;
    }
}
