using ExaminationSystem.API.Common;
using ExaminationSystem.API.Models.Requests.Courses;
using ExaminationSystem.API.Models.Responses;
using ExaminationSystem.Application.DTOs.Courses;
using ExaminationSystem.Application.Interfaces;
using ExaminationSystem.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq.Expressions;

namespace ExaminationSystem.API.Controllers;

/// <summary>
/// Controller for instructor-only course operations (add, update, delete).
/// </summary>
[Authorize(Roles = Constants.InstructorRoleName)]
public class CoursesController : BaseController
{
    private readonly ICourseService _courseService;

    /// <summary>
    /// Creates a new instance of <see cref="CoursesController"/>.
    /// </summary>
    /// <param name="courseService">Course service used to perform course operations.</param>
    public CoursesController(ICourseService courseService)
    {
        _courseService = courseService;
    }

    [HttpGet]
    [Authorize]
    public async Task<PaginatedResponse<CourseDto>> List([FromQuery] ListCoursesRequest request, CancellationToken cancellationToken = default)
    {
        var listDto = request.Adapt<ListCoursesDto>();

        var (courses, totalCount) = await _courseService.GetAll(listDto, cancellationToken);

        return new PaginatedResponse<CourseDto>(courses, totalCount);
    }

    /// <summary>
    /// Adds a new course for the current instructor.
    /// </summary>
    /// <param name="addCourseRequest">Course creation request DTO coming from the client.</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
    /// <returns>
    /// A <see cref="BaseResponse{T}"/> containing the new course id when successful,
    /// or a failure response with an error code and message on failure.
    /// </returns>
    [HttpPost]
    public async Task<BaseResponse<int>> Add(AddCourseRequest addCourseRequest, CancellationToken cancellationToken = default)
    {
        var addCourseDto = addCourseRequest.Adapt<AddCourseDto>();
        addCourseDto.InstructorID = CurrentUserId!.Value;

        var (result, id) = await _courseService.Add(addCourseDto, cancellationToken);

        return result == CourseOperationResult.Success
            ? new SuccessResponse<int>(id, CourseOperationResult.Success.ToString())
            : new FailureResponse<int>(ErrorCode.Error, result.ToString());
    }

    /// <summary>
    /// Updates information for an existing course.
    /// </summary>
    /// <param name="updateCourseRequest">Course update request DTO coming from the client.</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
    /// <returns>
    /// A <see cref="BaseResponse{T}"/> containing an empty success payload when the update succeeds,
    /// or a failure response with an error code and message on failure.
    /// </returns>
    [HttpPut]
    public async Task<BaseResponse<string>> Update(UpdateCourseRequest updateCourseRequest, CancellationToken cancellationToken = default)
    {
        var updateCourseDto = updateCourseRequest.Adapt<UpdateCourseDto>();

        var result = await _courseService.UpdateInfo(updateCourseDto, cancellationToken);

        return result == CourseOperationResult.Success
             ? new SuccessResponse<string>(string.Empty, CourseOperationResult.Success.ToString())
             : new FailureResponse<string>(ErrorCode.Error, result.ToString());
    }

    /// <summary>
    /// Deletes a course owned by the current instructor.
    /// </summary>
    /// <param name="courseId">Identifier of the course to delete.</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
    /// <returns>
    /// A <see cref="BaseResponse{T}"/> containing an empty success payload when the deletion succeeds,
    /// or a failure response with an error code and message on failure.
    /// </returns>
    [HttpDelete]
    public async Task<BaseResponse<string>> Delete(int courseId, CancellationToken cancellationToken = default)
    {
        var deleteCourseDto = new DeleteCourseDto
        {
            CourseId = courseId,
            ActorId = CurrentUserId!.Value
        };

        var result = await _courseService.Delete(deleteCourseDto, cancellationToken);

        return result == CourseOperationResult.Success
             ? new SuccessResponse<string>(string.Empty, CourseOperationResult.Success.ToString())
             : new FailureResponse<string>(ErrorCode.Error, result.ToString());
    }
}