using ExaminationSystem.API.Models.Requests.Questions;
using ExaminationSystem.API.Models.Responses;
using ExaminationSystem.Application.DTOs.Questions;
using ExaminationSystem.Application.Interfaces;
using MapsterMapper;
using Microsoft.AspNetCore.Mvc;

namespace ExaminationSystem.API.Controllers;

/// <summary>
/// Controller for managing Questions.
/// </summary>
public class QuestionsController : BaseController
{
    private readonly IQuestionService _questionService;

    public QuestionsController(IMapper mapper, IQuestionService questionService) : base(mapper)
    {
        _questionService = questionService;
    }

    /// <summary>
    /// Retrieves a paginated list of questions.
    /// </summary>
    /// <param name="start">The starting index for pagination. Default is 0.</param>
    /// <param name="length">The number of items to retrieve. Default is 10.</param>
    /// <returns>A list of questions.</returns>
    [HttpGet]
    public async Task<BaseResponse<IEnumerable<QuestionDto>>> List(int start = 0, int length = 10)
    {
        var questions = await _questionService.GetAll(start, length);

        return new SuccessResponse<IEnumerable<QuestionDto>>(questions);
    }

    /// <summary>
    /// Retrieves the details of a specific question by its ID.
    /// </summary>
    /// <param name="id">The ID of the question.</param>
    /// <returns>The details of the question if found; otherwise, an error response.</returns>
    [HttpGet]
    public async Task<BaseResponse<QuestionDto?>> GetDetails(int id)
    {
        var question = await _questionService.GetByID(id);

        if (question is null)
            return new FailureResponse<QuestionDto?>(ErrorCode.None, "Couldnt find that item");

        return new SuccessResponse<QuestionDto?>(question);
    }

    /// <summary>
    /// Adds a new question.
    /// </summary>
    /// <param name="request">The request model containing question details.</param>
    /// <returns>The added question.</returns>
    [HttpPost]
    public async Task<BaseResponse<QuestionDto>> Add(AddQuestionRequest request)
    {
        var addQuestionDto = _mapper.Map<AddQuestionDto>(request);
        var questionDto = await _questionService.Add(addQuestionDto);

        return new SuccessResponse<QuestionDto>(questionDto);
    }

    /// <summary>
    /// Updates an existing question.
    /// </summary>
    /// <param name="request">The request model containing updated question details.</param>
    /// <returns>The updated question if successful; otherwise, an error response.</returns>
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

    /// <summary>
    /// Deletes questions by their IDs.
    /// </summary>
    /// <param name="idsToDelete">A list of question IDs to delete.</param>
    /// <returns>A success response if all questions were deleted; otherwise, a failure response with the IDs that could not be deleted.</returns>
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
