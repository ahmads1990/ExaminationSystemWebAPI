using ExaminationSystem.API.Extensions;
using ExaminationSystem.API.Models.Requests.Exams;
using ExaminationSystem.API.Models.Responses;
using ExaminationSystem.Application.DTOs.Exams;
using ExaminationSystem.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ExaminationSystem.API.Controllers;

public class ExamsController : BaseController
{
    private readonly IExamService _examService;

    public ExamsController(IExamService examService)
    {
        _examService = examService;
    }

    /// <summary>
    /// Retrieves a paginated, sorted, and filtered list of exams.
    /// </summary>
    /// <param name="pageIndex"></param>
    /// <param name="pageSize"></param>
    /// <param name="orderBy"></param>
    /// <param name="sortDirection"></param>
    /// <param name="title"></param>
    /// <param name="examType"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpGet]
    public async Task<PaginatedResponse<ExamListDto>> List(int pageIndex, int pageSize,
        string? orderBy, string? sortDirection, string? title, ExamType? examType, CancellationToken cancellationToken = default)
    {
        var sortingDirection = sortDirection == "desc" ? SortingDirection.Descending : SortingDirection.Ascending;
        var (exams, totalCount) = await _examService.GetAll(pageIndex, pageSize, orderBy, sortingDirection, title, examType, cancellationToken);

        return new PaginatedResponse<ExamListDto>(exams, totalCount);
    }

    /// <summary>
    /// Retrieves the details of a specific exam by its ID.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpGet]
    public async Task<ApiResponse<ExamDto?>> GetDetails(int id, CancellationToken cancellationToken = default)
    {
        var exam = await _examService.GetByID(id, cancellationToken);

        if (exam is null)
            return new ErrorResponse<ExamDto?>(ApiErrorCode.ResourceNotFound, "Couldnt find that entity");

        return new SuccessResponse<ExamDto?>(exam);
    }

    /// <summary>
    /// Adds new exam
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpPost]
    public async Task<ApiResponse<ExamDto>> Add(AddExamRequest request, CancellationToken cancellationToken = default)
    {
        var addExamDto = request.Adapt<AddExamDto>();
        var result = await _examService.Add(addExamDto, cancellationToken);

        return new SuccessResponse<ExamDto>(result);
    }

    /// <summary>
    /// Update an existing exam
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpPut]
    public async Task<ApiResponse<ExamDto?>> Update(UpdateExamRequest request, CancellationToken cancellationToken = default)
    {
        var updateExamDto = request.Adapt<UpdateExamDto>();
        var examDto = await _examService.Update(updateExamDto, cancellationToken);

        if (examDto is null)
        {
            return new ErrorResponse<ExamDto?>(ApiErrorCode.ResourceNotFound, "Couldnt find that entity");
        }

        return new SuccessResponse<ExamDto?>(examDto);
    }

    /// <summary>
    /// Deletes questions by their IDs.
    /// </summary>
    /// <param name="idsToDelete"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpDelete]
    public async Task<ApiResponse<object>> Delete(List<int> idsToDelete, CancellationToken cancellationToken = default)
    {
        var unDeletedIds = await _examService.Delete(idsToDelete, cancellationToken);

        if (unDeletedIds is null || unDeletedIds.Any())
        {
            return new ErrorResponse<object>(ApiErrorCode.InsufficientPermissions, string.Join(',', idsToDelete));
        }

        return new SuccessResponse<object>(null);
    }

    /// <summary>
    /// Publishes an exam, making it visible and available to students.
    /// If no publish date is provided, it defaults to now.
    /// </summary>
    /// <param name="publishExamRequest">The publish request containing the exam ID and optional publish date.</param>
    /// <param name="cancellationToken"></param>
    /// <returns>A success or error response based on the operation result.</returns>
    [HttpPatch]
    public async Task<ApiResponse<string>> Publish(PublishExamRequest publishExamRequest, CancellationToken cancellationToken = default)
    {
        var publishExamDto = publishExamRequest.Adapt<PublishExamDto>();

        var result = await _examService.Publish(publishExamDto, cancellationToken);

        return result == ExamOperationResult.Success
            ? new SuccessResponse<string>("Exam published successfully.")
            : new ErrorResponse<string>(result.ToApiErrorCode());
    }

    /// <summary>
    /// Unpublishes an exam, reverting it to draft status and clearing its publish date.
    /// </summary>
    /// <param name="id">The ID of the exam to unpublish.</param>
    /// <param name="cancellationToken"></param>
    /// <returns>A success or error response based on the operation result.</returns>
    [HttpPatch]
    public async Task<ApiResponse<string>> UnPublish(int id, CancellationToken cancellationToken = default)
    {
        var result = await _examService.UnPublish(id, cancellationToken);

        return result == ExamOperationResult.Success
            ? new SuccessResponse<string>("Exam unpublished successfully.")
            : new ErrorResponse<string>(result.ToApiErrorCode());
    }
}
