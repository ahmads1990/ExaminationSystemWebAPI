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
    public async Task<BaseResponse<ExamDto?>> GetDetails(int id, CancellationToken cancellationToken = default)
    {
        var exam = await _examService.GetByID(id, cancellationToken);

        if (exam is null)
            return new FailureResponse<ExamDto?>(ErrorCode.EntityNotFound, "Couldnt find that entity");

        return new SuccessResponse<ExamDto?>(exam);
    }

    /// <summary>
    /// Adds new exam
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpPost]
    public async Task<BaseResponse<ExamDto>> Add(AddExamRequest request, CancellationToken cancellationToken = default)
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
    public async Task<BaseResponse<ExamDto?>> Update(UpdateExamRequest request, CancellationToken cancellationToken = default)
    {
        var updateExamDto = request.Adapt<UpdateExamDto>();
        var examDto = await _examService.Update(updateExamDto, cancellationToken);

        if (examDto is null)
        {
            return new FailureResponse<ExamDto?>(ErrorCode.EntityNotFound, "Couldnt find that entity");
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
    public async Task<BaseResponse<object>> Delete(List<int> idsToDelete, CancellationToken cancellationToken = default)
    {
        var unDeletedIds = await _examService.Delete(idsToDelete, cancellationToken);

        if (unDeletedIds is null || unDeletedIds.Any())
        {
            return new FailureResponse<object>(ErrorCode.CannotDelete, string.Join(',', idsToDelete));
        }

        return new SuccessResponse<object>("Deleted succuesfully");
    }
}
