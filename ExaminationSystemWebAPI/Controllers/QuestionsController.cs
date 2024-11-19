using Microsoft.AspNetCore.Mvc;

namespace ExaminationSystemWebAPI.Controllers;

[ApiController]
[Route("[controller]/[action]")]
public class QuestionsController : ControllerBase
{
    [HttpGet]
    public IActionResult GetAll()
    {
        return Ok();
    }

    [HttpGet]
    public IActionResult GetByID(string id)
    {
        return Ok();
    }

    [HttpPost]
    public IActionResult Create()
    {
        return Ok();
    }

    [HttpPut]
    public IActionResult UpdateQuestion()
    {
        return Ok();
    }

    [HttpPatch]
    public IActionResult UpdateLevel()
    {
        return Ok();
    }

    [HttpPatch]
    public IActionResult UpdateBody()
    {
        return Ok();
    }

    [HttpPatch]
    public IActionResult UpdateScore()
    {
        return Ok();
    }

    [HttpDelete]
    public IActionResult DeleteQuestion(string id)
    {
        return Ok();
    }
}
