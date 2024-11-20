using ExaminationSystemWebAPI.Models;
using ExaminationSystemWebAPI.Services.ChoiceService;
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
    private readonly IChoiceService _choiceService;

    public QuestionsController(IQuestionService questionService, IChoiceService choiceService)
    {
        _questionService = questionService;
        _choiceService = choiceService;
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
    public async Task<IActionResult> GetByID([FromQuery]string id)
    {
        var result = await _questionService.GetByID(id);

        return Ok(result);
    }

    [HttpPost]
    public IActionResult Create(AddQuestionViewModel viewModel)
    {
        var question = viewModel.Adapt<Question>();

        foreach (var choice in question.Choices)
        {
            _choiceService.Add(choice);
        }

        _questionService.Add(question);
        _questionService.SaveChanges();

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
        var question = new Question { ID = id };

        _questionService.Delete(question);
        _questionService.SaveChanges();
        return Ok();
    }
}
