using ExaminationSystemWebAPI.Models;
using ExaminationSystemWebAPI.Services.QuestionService;
using ExaminationSystemWebAPI.ViewModels.Questions;
using Mapster;
using Microsoft.AspNetCore.Mvc;

namespace ExaminationSystemWebAPI.Controllers;

[ApiController]
[Route("[controller]/[action]")]
public class QuestionsController : ControllerBase
{
    private readonly IQuestionService _questionService;

    public QuestionsController(IQuestionService questionService)
    {
        _questionService = questionService;
    }

    [HttpGet]
    public IActionResult GetAll()
    {
        var result = _questionService
            .GetAll()
            .ProjectToType<QuestionViewModel>()
            .ToList();

        return Ok(result);
    }

    [HttpGet]
    public async Task<IActionResult> GetByID([FromQuery] string id)
    {
        var result = await _questionService.GetByID(id);

        return Ok(result);
    }

    [HttpPost]
    public IActionResult Create(AddQuestionViewModel viewModel)
    {
        var question = viewModel.Adapt<Question>();

        _questionService.Add(question);
        _questionService.SaveChanges();

        return Ok();
    }

    [HttpPut]
    public IActionResult UpdateQuestion(UpdateQuestionViewModel viewModel)
    {
        var question = viewModel.Adapt<Question>();

        _questionService.UpdateQuestion(question);

        _questionService.SaveChanges();
        return Ok();
    }

    [HttpPatch]
    public IActionResult UpdateLevel(UpdateQuestionLevelViewModel viewModel)
    {
        var question = viewModel.Adapt<Question>();

        _questionService.UpdateLevel(question);

        _questionService.SaveChanges();
        return Ok();
    }

    [HttpPatch]
    public IActionResult UpdateBody(UpdateQuestionBodyViewModel viewModel)
    {
        var question = viewModel.Adapt<Question>();

        _questionService.UpdateBody(question);

        _questionService.SaveChanges();
        return Ok();
    }

    [HttpPatch]
    public IActionResult UpdateScore(UpdateQuestionScoreViewModel viewModel)
    {
        var question = viewModel.Adapt<Question>();

        _questionService.UpdateScore(question);

        _questionService.SaveChanges();
        return Ok();
    }

    [HttpDelete]
    public IActionResult DeleteQuestion(string id)
    {
        var question = new Question { ID = id };

        _questionService.Delete(question);
        _questionService.SaveChanges();
        return Ok();
    }
}
