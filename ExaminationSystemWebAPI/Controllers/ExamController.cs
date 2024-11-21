using ExaminationSystemWebAPI.Models;
using ExaminationSystemWebAPI.Services.ExamService;
using ExaminationSystemWebAPI.ViewModels.Exams;
using Mapster;
using Microsoft.AspNetCore.Mvc;

namespace ExaminationSystemWebAPI.Controllers;

[ApiController]
[Route("[controller]/[action]")]
public class ExamController : ControllerBase
{
    private readonly IExamService _examService;

    public ExamController(IExamService examService)
    {
        _examService = examService;
    }

    [HttpGet]
    public IActionResult GetAll()
    {
        var result = _examService
            .GetAll()
            .ProjectToType<ExamViewModel>()
            .ToList();

        return Ok(result);
    }

    [HttpGet]
    public async Task<IActionResult> GetByID([FromQuery] string id)
    {
        var result = await _examService
            .GetByID(id);

        return Ok(result);
    }

    [HttpDelete]
    public IActionResult Delete([FromQuery] string id)
    {
        var exam = new Exam { ID = id };

        _examService.Delete(exam);
        _examService.SaveChanges();

        return Ok();
    }
}
