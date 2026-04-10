using ExaminationSystem.API.Extensions;
using ExaminationSystem.API.Models.Requests.Exams;
using ExaminationSystem.API.Models.Responses;
using ExaminationSystem.Application.DTOs;
using ExaminationSystem.Application.DTOs.Exams;
using ExaminationSystem.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ExaminationSystem.API.Common;

namespace ExaminationSystem.API.Controllers;

/// <summary>
/// Controller for managing exams, including CRUD operations, publishing, and question assignment.
/// </summary>
[Authorize(Roles = Constants.InstructorRoleName)]
public class ExamsController : BaseController
{
    #region Fields

    private readonly IExamService _examService;

    #endregion

    #region Constructors

    /// <summary>
    /// Initializes a new instance of the <see cref="ExamsController"/> class.
    /// </summary>
    /// <param name="examService">The exam service.</param>
    public ExamsController(IExamService examService)
    {
        _examService = examService;
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Retrieves a paginated, sorted, and filtered list of exams.
    /// </summary>
    /// <param name="request">The search, pagination, and sorting criteria.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A paginated response containing the list of exams.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(PaginatedResponse<ExamListDto>), StatusCodes.Status200OK)]
    public async Task<PaginatedResponse<ExamListDto>> List([FromQuery] ListExamsRequest request, CancellationToken cancellationToken = default)
    {
        var listDto = request.Adapt<ListExamsDto>();
        var (exams, totalCount) = await _examService.GetAll(listDto, cancellationToken);

        return new PaginatedResponse<ExamListDto>(exams, totalCount);
    }

    /// <summary>
    /// Retrieves the details of a specific exam by its ID.
    /// </summary>
    /// <param name="id">The exam identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The exam details if found, otherwise an error response.</returns>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(SuccessResponse<ExamDto?>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse<ExamDto?>), StatusCodes.Status404NotFound)]
    public async Task<ApiResponse<ExamDto?>> GetDetails(int id, CancellationToken cancellationToken = default)
    {
        var exam = await _examService.GetByID(id, cancellationToken);

        return exam is null
            ? new ErrorResponse<ExamDto?>(ApiErrorCode.ExamNotFound)
            : new SuccessResponse<ExamDto?>(exam);
    }

    /// <summary>
    /// Adds new exam.
    /// </summary>
    /// <param name="request">The exam creation request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A success response with the new exam ID, or an error response.</returns>
    [HttpPost]
    [ProducesResponseType(typeof(SuccessResponse<int>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse<int>), StatusCodes.Status400BadRequest)]
    public async Task<ApiResponse<int>> Add(AddExamRequest request, CancellationToken cancellationToken = default)
    {
        var addExamDto = request.Adapt<AddExamDto>();
        var (result, id) = await _examService.Add(addExamDto, cancellationToken);

        return result == ExamOperationResult.Success
            ? new SuccessResponse<int>(id)
            : new ErrorResponse<int>(result.ToApiErrorCode());
    }

    /// <summary>
    /// Updates an existing exam's settings.
    /// </summary>
    /// <param name="request">The request containing updated exam data.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A success or error response based on the operation result.</returns>
    [HttpPut]
    [ProducesResponseType(typeof(SuccessResponse<string>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse<string>), StatusCodes.Status400BadRequest)]
    public async Task<ApiResponse<string>> Update(UpdateExamRequest request, CancellationToken cancellationToken = default)
    {
        var updateExamDto = request.Adapt<UpdateExamDto>();
        var result = await _examService.Update(updateExamDto, cancellationToken);

        return result == ExamOperationResult.Success
            ? new SuccessResponse<string>(string.Empty)
            : new ErrorResponse<string>(result.ToApiErrorCode());
    }

    /// <summary>
    /// Deletes exams by their IDs.
    /// </summary>
    /// <param name="idsToDelete">List of exam IDs to delete.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A success response if all exams were deleted, otherwise an error response.</returns>
    [HttpDelete]
    [ProducesResponseType(typeof(SuccessResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse<object>), StatusCodes.Status403Forbidden)]
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
    /// Deletes a single exam by its ID.
    /// Deletion is blocked when the exam is published or has student submissions.
    /// </summary>
    /// <param name="id">The exam identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A success response, or an error response with the reason for failure.</returns>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(typeof(SuccessResponse<string>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse<string>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse<string>), StatusCodes.Status400BadRequest)]
    public async Task<ApiResponse<string>> DeleteById(int id, CancellationToken cancellationToken = default)
    {
        var result = await _examService.DeleteById(id, cancellationToken);

        return result == ExamOperationResult.Success
            ? new SuccessResponse<string>(string.Empty)
            : new ErrorResponse<string>(result.ToApiErrorCode());
    }

    /// <summary>
    /// Publishes an exam, making it visible and available to students.
    /// If no publish date is provided, it defaults to now.
    /// </summary>
    /// <param name="publishExamRequest">The publish request containing the exam ID and optional publish date.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A success or error response based on the operation result.</returns>
    [HttpPatch("publish")]
    [ProducesResponseType(typeof(SuccessResponse<string>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse<string>), StatusCodes.Status400BadRequest)]
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
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A success or error response based on the operation result.</returns>
    [HttpPatch("{id:int}/unpublish")]
    [ProducesResponseType(typeof(SuccessResponse<string>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse<string>), StatusCodes.Status400BadRequest)]
    public async Task<ApiResponse<string>> UnPublish(int id, CancellationToken cancellationToken = default)
    {
        var result = await _examService.UnPublish(id, cancellationToken);

        return result == ExamOperationResult.Success
            ? new SuccessResponse<string>("Exam unpublished successfully.")
            : new ErrorResponse<string>(result.ToApiErrorCode());
    }

    /// <summary>
    /// Assigns questions to an exam. Returns a list of rejected questions with reasons.
    /// </summary>
    /// <param name="request">The exam ID and question IDs to assign.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A success response with rejected items, or an error for exam-level failures.</returns>
    [HttpPatch("assign-questions")]
    [ProducesResponseType(typeof(SuccessResponse<IEnumerable<RejectedEntityDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse<IEnumerable<RejectedEntityDto>>), StatusCodes.Status400BadRequest)]
    public async Task<ApiResponse<IEnumerable<RejectedEntityDto>>> AssignQuestions(AssignQuestionsRequest request, CancellationToken cancellationToken = default)
    {
        var dto = request.Adapt<AssignQuestionsDto>();
        var (result, rejected) = await _examService.AssignQuestions(dto, cancellationToken);

        return result == ExamOperationResult.Success
            ? new SuccessResponse<IEnumerable<RejectedEntityDto>>(rejected)
            : new ErrorResponse<IEnumerable<RejectedEntityDto>>(result.ToApiErrorCode());
    }

    /// <summary>
    /// Unassigns questions from an exam. Returns a list of rejected questions with reasons.
    /// </summary>
    /// <param name="request">The exam ID and question IDs to unassign.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A success response with rejected items, or an error for exam-level failures.</returns>
    [HttpPatch("unassign-questions")]
    [ProducesResponseType(typeof(SuccessResponse<IEnumerable<RejectedEntityDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse<IEnumerable<RejectedEntityDto>>), StatusCodes.Status400BadRequest)]
    public async Task<ApiResponse<IEnumerable<RejectedEntityDto>>> UnassignQuestions(AssignQuestionsRequest request, CancellationToken cancellationToken = default)
    {
        var dto = request.Adapt<AssignQuestionsDto>();
        var (result, rejected) = await _examService.UnassignQuestions(dto, cancellationToken);

        return result == ExamOperationResult.Success
            ? new SuccessResponse<IEnumerable<RejectedEntityDto>>(rejected)
            : new ErrorResponse<IEnumerable<RejectedEntityDto>>(result.ToApiErrorCode());
    }

    #endregion
}
