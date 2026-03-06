using ExaminationSystem.Application.DTOs.Courses;
using ExaminationSystem.Application.Interfaces;
using ExaminationSystem.Domain.Common;
using ExaminationSystem.Domain.Entities;
using ExaminationSystem.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace ExaminationSystem.Application.Services;

public class CourseService : ICourseService
{
    #region Fields

    private readonly IRepository<Course> _courseRepository;

    #endregion

    #region Constructors

    public CourseService(IRepository<Course> courseRepository)
    {
        _courseRepository = courseRepository;
    }

    #endregion

    #region Public Methods

    /// <inheritdoc/>
    public async Task<(IEnumerable<CourseDto> Data, int TotalCount)> GetAll(ListCoursesDto listDto, CancellationToken cancellationToken = default)
    {
        var query = _courseRepository.GetAll();
        query = ApplySearchFilters(query, listDto);

        Expression<Func<Course, object>> sortingExpression = listDto.OrderBy switch
        {
            nameof(Course.InstructorID) => q => q.InstructorID,
            nameof(Course.CreditHours) => q => q.CreditHours,
            nameof(Course.ID) => q => q.ID,
            _ => q => q.CreatedDate
        };

        query = listDto.SortDirection == SortingDirection.Ascending
                    ? query.OrderBy(sortingExpression)
                    : query.OrderByDescending(sortingExpression);

        var totalCount = await query.CountAsync(cancellationToken);

        var data = await query.Skip(listDto.PageIndex * listDto.PageSize).Take(listDto.PageSize)
                      .ProjectToType<CourseDto>()
                      .ToListAsync(cancellationToken);

        return (data, totalCount);
    }

    /// <inheritdoc/>
    public async Task<(CourseOperationResult Result, int Id)> Add(AddCourseDto courseDto, CancellationToken cancellationToken = default)
    {
        // Validate required fields
        if (string.IsNullOrEmpty(courseDto.Title) ||
            string.IsNullOrEmpty(courseDto.Description) ||
            courseDto.CreditHours <= 0 ||
            courseDto.InstructorID == 0)
        {
            return (CourseOperationResult.ValidationFailed, 0);
        }

        if (await HasInstructorExceededCourseLimit(courseDto.InstructorID, cancellationToken))
            return (CourseOperationResult.MaxCoursesExceeded, 0);

        var duplicateCourse = await _courseRepository.CheckExistsByCondition(c => EF.Functions.Like(c.Title, courseDto.Title), cancellationToken);
        if (duplicateCourse)
            return (CourseOperationResult.DuplicateTitle, 0);

        var course = courseDto.Adapt<Course>();

        await _courseRepository.Add(course, cancellationToken);
        await _courseRepository.SaveChanges(cancellationToken);

        return (CourseOperationResult.Success, course.ID);
    }

    /// <inheritdoc/>
    public async Task<CourseOperationResult> UpdateInfo(UpdateCourseDto courseDto, CancellationToken cancellationToken = default)
    {
        if (!await _courseRepository.CheckExistsByID(courseDto.ID, cancellationToken))
            return CourseOperationResult.NotFound;

        var duplicateCourse = await _courseRepository.CheckExistsByCondition(c => c.ID != courseDto.ID && EF.Functions.Like(c.Title, courseDto.Title), cancellationToken);
        if (duplicateCourse)
            return CourseOperationResult.DuplicateTitle;

        var course = courseDto.Adapt<Course>();
        _courseRepository.SaveInclude(course, nameof(Course.Title), nameof(Course.Description), nameof(Course.CreditHours));

        await _courseRepository.SaveChanges(cancellationToken);
        return CourseOperationResult.Success;
    }

    /// <inheritdoc/>
    public async Task<CourseOperationResult> Delete(DeleteCourseDto courseDto, CancellationToken cancellationToken = default)
    {
        // Check if instructor owns this course
        var course = await _courseRepository.GetByCondition(c => c.ID == courseDto.CourseId)
                                            .Select(c => new { c.ID, c.InstructorID })
                                            .FirstOrDefaultAsync(cancellationToken);

        if (course is null)
            return CourseOperationResult.NotFound;
        else if (course.InstructorID != courseDto.ActorId)
            return CourseOperationResult.NotOwner;

        var stub = new Course() { ID = course.ID, Instructor = default };
        _courseRepository.SoftDelete(stub);
        await _courseRepository.SaveChanges(cancellationToken);

        return CourseOperationResult.Success;
    }

    #endregion

    #region Private Methods

    /// <summary>
    /// Checks if the specified instructor has reached the maximum number of allowed courses.
    /// </summary>
    /// <param name="instructorId">The unique identifier of the instructor.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns><see langword="true"/> if the instructor has reached or exceeded the limit; otherwise, <see langword="false"/>.</returns>
    private async Task<bool> HasInstructorExceededCourseLimit(int instructorId, CancellationToken cancellationToken = default)
    {
        return await _courseRepository.GetByCondition(c => c.InstructorID == instructorId)
                                      .CountAsync(cancellationToken) >= Constants.MaxAllowedCoursesPerInstructor;
    }

    /// <summary>
    /// Applies search filters to the specified course query based on the criteria provided in the filter DTO.
    /// </summary>
    /// <param name="query">The initial queryable collection of courses to filter.</param>
    /// <param name="listDto">An object containing search criteria.</param>
    /// <returns>An IQueryable of Course representing the filtered set of courses.</returns>
    private IQueryable<Course> ApplySearchFilters(IQueryable<Course> query, ListCoursesDto listDto)
    {
        if (!string.IsNullOrEmpty(listDto.Title))
            query = query.Where(c => c.Title != null && c.Title.Contains(listDto.Title));

        if (!string.IsNullOrEmpty(listDto.Description))
            query = query.Where(c => c.Description != null && c.Description.Contains(listDto.Description));

        if (listDto.CreditHours.HasValue)
            query = query.Where(c => c.CreditHours == listDto.CreditHours.Value);

        if (listDto.InstructorID.HasValue)
            query = query.Where(c => c.InstructorID == listDto.InstructorID.Value);

        return query;
    }

    #endregion
}
