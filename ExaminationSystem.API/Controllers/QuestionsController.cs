using ExaminationSystem.API.Models.Requests.Questions;
using ExaminationSystem.Application.Interfaces;
using ExaminationSystem.Application.Models.Requests.Questions;
using MapsterMapper;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace ExaminationSystem.API.Controllers;

public class QuestionsController : BaseController
{
    private readonly IQuestionService _questionService;

    public QuestionsController(IMapper mapper, IQuestionService questionService) : base(mapper)
    {
        _questionService = questionService;
    }

    [HttpGet]
    public IActionResult List()
    {

    }

    [HttpPost]
    public async Task<IActionResult> Add(AddQuestionRequest request)
    {
        await _questionService.Add(request);
    }
}
