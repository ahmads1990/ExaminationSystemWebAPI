using ExaminationSystem.API.Models.Requests.Questions;
using ExaminationSystem.API.Models.Responses;
using ExaminationSystem.Application.DTOs.Questions;
using ExaminationSystem.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ExaminationSystem.API.Controllers;

/// <summary>
/// Controller for managing Questions.
/// </summary>
public class QuestionsController : BaseController
{
    private readonly IQuestionService _questionService;

    public QuestionsController(IQuestionService questionService) : base()
    {
        _questionService = questionService;
    }

    /// <summary>
    /// Retrieves a paginated, sorted, and filtered list of questions.
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpGet]
    public async Task<PaginatedResponse<QuestionDto>> List([FromQuery] ListQuestionsRequest request, CancellationToken cancellationToken = default)
    {
        var listDto = request.Adapt<ListQuestionsDto>();
        var (questions, totalCount) = await _questionService.GetAll(listDto, cancellationToken);

        return new PaginatedResponse<QuestionDto>(questions, totalCount);
    }

    /// <summary>
    /// Retrieves the details of a specific question by its ID.
    /// </summary>
    /// <param name="id">The ID of the question.</param>
    /// <param name="cancellationToken">Optional cancellation token for user to cancel the request</param>
    /// <returns>The details of the question if found; otherwise, an error response.</returns>
    [HttpGet]
    public async Task<ApiResponse<QuestionDto?>> GetDetails(int id, CancellationToken cancellationToken = default)
    {
        var question = await _questionService.GetByID(id, cancellationToken);

        return question is null
            ? new ErrorResponse<QuestionDto?>(ApiErrorCode.QuestionNotFound)
            : new SuccessResponse<QuestionDto?>(question);
    }

    /// <summary>
    /// Adds a new question.
    /// </summary>
    /// <param name="request">The request model containing question details.</param>
    /// <param name="cancellationToken">Optional cancellation token for user to cancel the request</param>
    /// <returns>The added question.</returns>
    [HttpPost]
    public async Task<ApiResponse<object>> Add(AddQuestionRequest request, CancellationToken cancellationToken = default)
    {
        var addQuestionDto = request.Adapt<AddQuestionDto>();
        var result = await _questionService.Add(addQuestionDto, cancellationToken);

        if (result != QuestionOperationResult.Success)
            return new ErrorResponse<object>(result.ToApiErrorCode());

        return new SuccessResponse<object>(null);
    }

    /// <summary>
    /// Updates an existing question.
    /// </summary>
    /// <param name="request">The request model containing updated question details.</param>
    /// <param name="cancellationToken">Optional cancellation token for user to cancel the request</param>
    /// <returns>The updated question if successful; otherwise, an error response.</returns>
    [HttpPut]
    public async Task<ApiResponse<object>> Update(UpdateQuestionRequest request, CancellationToken cancellationToken = default)
    {
        var updateQuestionDto = request.Adapt<UpdateQuestionDto>();
        var result = await _questionService.Update(updateQuestionDto, cancellationToken);

        if (result != QuestionOperationResult.Success)
            return new ErrorResponse<object>(result.ToApiErrorCode());

        return new SuccessResponse<object>(null);
    }

    /// <summary>
    /// Deletes questions by their IDs.
    /// </summary>
    /// <param name="idsToDelete">A list of question IDs to delete.</param>
    /// <returns>A success response if all questions were deleted; otherwise, a failure response with the IDs that could not be deleted.</returns>
    [HttpDelete]
    public async Task<ApiResponse<object>> Delete(List<int> idsToDelete, CancellationToken cancellationToken = default)
    {
        var unDeletedIds = await _questionService.Delete(idsToDelete, cancellationToken);

        if (unDeletedIds is null || unDeletedIds.Any())
        {
            return new ErrorResponse<object>(ApiErrorCode.InsufficientPermissions, string.Join(',', idsToDelete));
        }

        return new SuccessResponse<object>(null);
    }
}
