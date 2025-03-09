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
    public async Task<BaseResponse<IEnumerable<QuestionDto>>> List(int start = 0, int length = 10)
    {
        var questions = await _questionService.GetAll(start, length);

        return new SuccessResponse<IEnumerable<QuestionDto>>(questions);
    }

    [HttpGet]
    public async Task<BaseResponse<QuestionDto?>> GetDetails(int id)
    {
        var question = await _questionService.GetByID(id);

        if (question is null)
            return new FailureResponse<QuestionDto?>(ErrorCode.None, "Couldnt find that item");

        return new SuccessResponse<QuestionDto?>(question);
    }

    [HttpPost]
    public async Task<BaseResponse<QuestionDto>> Add(AddQuestionRequest request)
    {
        var addQuestionDto = _mapper.Map<AddQuestionDto>(request);
        var questionDto = await _questionService.Add(addQuestionDto);

        return new SuccessResponse<QuestionDto>(questionDto);
    }

    [HttpPut]
    public async Task<BaseResponse<QuestionDto?>> Update(UpdateQuestionRequest request)
    {
        var updateQuestionDto = _mapper.Map<UpdateQuestionDto>(request);
        var questionDto = await _questionService.Update(updateQuestionDto);

        if (questionDto is null)
        {
            return new FailureResponse<QuestionDto?>(ErrorCode.UnKnownError, "Couldnt find that item, invalid id");
        }

        return new SuccessResponse<QuestionDto?>(questionDto);
    }

    [HttpDelete]
    public async Task<BaseResponse<object>> Delete(List<int> idsToDelete)
    {
        var unDeletedIds = await _questionService.Delete(idsToDelete);

        if (unDeletedIds is null || unDeletedIds.Any())
        {
            return new FailureResponse<object>(ErrorCode.UnKnownError, string.Join(',', idsToDelete));
        }

        return new SuccessResponse<object>("Deleted succuesfully");
    }
}
