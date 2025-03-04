using ExaminationSystem.API.Models.Requests.Questions;
using ExaminationSystem.API.Models.Responses;
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

    [HttpGet]
    public Task<BaseResponse<IEnumerable<QuestionDto>>> List(int start, int length = 10)
    {

    }

    [HttpGet]
    public Task<BaseResponse<QuestionDto>> GetDetails(int id)
    {

    }

    [HttpPost]
    public async Task<BaseResponse<QuestionDto>> Add(AddQuestionRequest request)
    {
        var addQuestionDto = _mapper.Map<AddQuestionDto>(request);
        var questionDto = await _questionService.Add(addQuestionDto);

        return new SuccessResponse<QuestionDto>(questionDto);
    }

    [HttpPut]
    public async Task<BaseResponse<QuestionDto>> Update(UpdateQuestionRequest request)
    {

    }

    [HttpDelete]
    public async Task<BaseResponse<List<int>>> Delete(List<int> idsToDelete)
    {

    }
}
