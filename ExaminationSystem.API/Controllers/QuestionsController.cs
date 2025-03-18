﻿using ExaminationSystem.API.Models.Requests.Questions;
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
    /// <param name="pageIndex"></param>
    /// <param name="pageSize"></param>
    /// <param name="orderBy"></param>
    /// <param name="sortDirection"></param>
    /// <param name="examId"></param>
    /// <param name="body"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpGet]
    public async Task<PaginatedResponse<QuestionDto>> List(int pageIndex, int pageSize,
        string? orderBy, string? sortDirection, int? examId, string? body, CancellationToken cancellationToken = default)
    {
        var sortingDirection = sortDirection == "desc" ? SortingDirection.Descending : SortingDirection.Ascending;
        var (questions, totalCount) = await _questionService.GetAll(pageIndex, pageSize, examId, orderBy, sortingDirection, body);

        return new PaginatedResponse<QuestionDto>(questions, totalCount);
    }

    /// <summary>
    /// Retrieves the details of a specific question by its ID.
    /// </summary>
    /// <param name="id">The ID of the question.</param>
    /// <param name="cancellationToken">Optional cancellation token for user to cancel the request</param>
    /// <returns>The details of the question if found; otherwise, an error response.</returns>
    [HttpGet]
    public async Task<BaseResponse<QuestionDto?>> GetDetails(int id, CancellationToken cancellationToken = default)
    {
        var question = await _questionService.GetByID(id, cancellationToken);

        if (question is null)
            return new FailureResponse<QuestionDto?>(ErrorCode.EntityNotFound, "Couldnt find that entity");

        return new SuccessResponse<QuestionDto?>(question);
    }

    /// <summary>
    /// Adds a new question.
    /// </summary>
    /// <param name="request">The request model containing question details.</param>
    /// <param name="cancellationToken">Optional cancellation token for user to cancel the request</param>
    /// <returns>The added question.</returns>
    [HttpPost]
    public async Task<BaseResponse<QuestionDto>> Add(AddQuestionRequest request, CancellationToken cancellationToken = default)
    {
        var addQuestionDto = request.Adapt<AddQuestionDto>();
        var questionDto = await _questionService.Add(addQuestionDto, cancellationToken);

        return new SuccessResponse<QuestionDto>(questionDto);
    }

    /// <summary>
    /// Updates an existing question.
    /// </summary>
    /// <param name="request">The request model containing updated question details.</param>
    /// <param name="cancellationToken">Optional cancellation token for user to cancel the request</param>
    /// <returns>The updated question if successful; otherwise, an error response.</returns>
    [HttpPut]
    public async Task<BaseResponse<QuestionDto?>> Update(UpdateQuestionRequest request, CancellationToken cancellationToken = default)
    {
        var updateQuestionDto = request.Adapt<UpdateQuestionDto>();
        var questionDto = await _questionService.Update(updateQuestionDto, cancellationToken);

        if (questionDto is null)
        {
            return new FailureResponse<QuestionDto?>(ErrorCode.EntityNotFound, "Couldnt find that entity");
        }

        return new SuccessResponse<QuestionDto?>(questionDto);
    }

    /// <summary>
    /// Deletes questions by their IDs.
    /// </summary>
    /// <param name="idsToDelete">A list of question IDs to delete.</param>
    /// <returns>A success response if all questions were deleted; otherwise, a failure response with the IDs that could not be deleted.</returns>
    [HttpDelete]
    public async Task<BaseResponse<object>> Delete(List<int> idsToDelete, CancellationToken cancellationToken = default)
    {
        var unDeletedIds = await _questionService.Delete(idsToDelete, cancellationToken);

        if (unDeletedIds is null || unDeletedIds.Any())
        {
            return new FailureResponse<object>(ErrorCode.CannotDelete, string.Join(',', idsToDelete));
        }

        return new SuccessResponse<object>("Deleted succuesfully");
    }
}
