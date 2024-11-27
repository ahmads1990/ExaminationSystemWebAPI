using ExaminationSystemWebAPI.Models;
using ExaminationSystemWebAPI.Services.ExamService;
using ExaminationSystemWebAPI.Services.QuestionService;
using ExaminationSystemWebAPI.ViewModels.Exams;
using Mapster;
using Microsoft.AspNetCore.Mvc;

namespace ExaminationSystemWebAPI.Controllers;

[ApiController]
[Route("[controller]/[action]")]
public class ExamController : ControllerBase
{
    private readonly IExamService _examService;
    private readonly IQuestionService _questionService;

    public ExamController(IExamService examService, IQuestionService questionService)
    {
        _examService = examService;
        _questionService = questionService;
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

    [HttpPost]
    public IActionResult CreateExam(AddExamViewModel viewModel)
    {
        var exam = viewModel.Adapt<Exam>();

        _examService.AddExam(exam);

        _examService.SaveChanges();
        return Ok();
    }

    [HttpPost]
    public IActionResult CreateFullExam(AddFullExamViewModel viewModel)
    {
        var exam = viewModel.Adapt<Exam>();

        //foreach (var question in exam.Questions)
        //{
        //    _questionService.Add(question);
        //}

        _examService.AddFullExam(exam);
        _examService.SaveChanges();
        return Ok();
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
