using ExaminationSystem.API.Models.Requests.Questions;
using ExaminationSystem.Application.DTOs.Questions;
using ExaminationSystem.Application.Interfaces;
using MapsterMapper;
using Microsoft.AspNetCore.Mvc;

namespace ExaminationSystem.API.Controllers;

public class QuestionsController : BaseController
{
    private readonly IQuestionService _questionService;

    public QuestionsController(IMapper mapper, IQuestionService questionService) : base(mapper)
    {
        _questionService = questionService;
    }

    //[HttpGet]
    //public IActionResult List()
    //{

    //}

    [HttpPost]
    public async Task<IActionResult> Add(AddQuestionRequest request)
    {
        var addQuestionDto = _mapper.Map<AddQuestionDto>(request);
        var questionDto = await _questionService.Add(addQuestionDto);

        return Ok(questionDto);
    }
}
