using ExaminationSystem.Application.DTOs.StudentCourses;
using ExaminationSystem.Application.Interfaces;
using ExaminationSystem.Domain.Common;
using ExaminationSystem.Domain.Entities;
using ExaminationSystem.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace ExaminationSystem.Application.Services;

public class StudentCourseService : IStudentCourseService
{
    #region Fields

    private IRepository<Course> _courseRepository;
    private IRepository<StudentCourses> _studentCoursesRepository;

    #endregion

    #region Public Methods

    /// <inheritdoc />
    public StudentCourseService(IRepository<Course> courseRepository, IRepository<StudentCourses> studentCoursesRepository)
    {
        _courseRepository = courseRepository;
        _studentCoursesRepository = studentCoursesRepository;
    }

    /// <inheritdoc />
    public async Task<(IEnumerable<StudentEnrollmentDto> Data, int TotalCount)> ListStudentEnrollments(ListStudentEnrollmentsDto listDto, CancellationToken cancellationToken = default)
    {
        var query = _studentCoursesRepository.GetAll()
                                             .Where(sc => sc.StudentID == listDto.StudentId);
        query = ApplySearchFilters(query, listDto);

        // Get count here
        var totalCount = await query.CountAsync();

        Expression<Func<StudentCourses, object>> sortingExpression = q => q.CreatedDate;
        if (!string.IsNullOrEmpty(listDto.OrderBy))
        {
            if (listDto.OrderBy.Equals(nameof(StudentCourses.EnrollmentDate)))
                sortingExpression = q => q.EnrollmentDate;
            else if (listDto.OrderBy.Equals(nameof(StudentCourses.Finished)))
                sortingExpression = q => q.Finished;
            else
                throw new ArgumentException($"Invalid orderBy field: {listDto.OrderBy}");
        }

        query = listDto.SortDirection == SortingDirection.Ascending
            ? query.OrderBy(sortingExpression)
            : query.OrderByDescending(sortingExpression);

        var data = await query.Skip(listDto.PageIndex * listDto.PageSize).Take(listDto.PageSize)
              .ProjectToType<StudentEnrollmentDto>()
              .ToListAsync(cancellationToken);

        return (data, totalCount);
    }

    /// <inheritdoc />
    public async Task<StudentCourseOperationResult> EnrollInCourse(StudentEnrollInCourseDto dto, CancellationToken cancellationToken = default)
    {
        // Check if the course exists
        var course = await _courseRepository.CheckExistsByID(dto.CourseId, cancellationToken);
        if (!course)
        {
            return StudentCourseOperationResult.CourseNotFound;
        }

        var studentEnrollments = await _studentCoursesRepository.GetAll()
                                                                .Where(sc => sc.StudentID == dto.StudentId)
                                                                .Select(sc => sc.CourseID)
                                                                .ToListAsync(cancellationToken);

        if (studentEnrollments.Contains(dto.CourseId))
        {
            return StudentCourseOperationResult.AlreadyEnrolled;
        }
        else if (studentEnrollments.Count >= Constants.MaxAllowedEnrolledCoursesPerStudent)
        {
            return StudentCourseOperationResult.MaxEnrollmentsExceeded;
        }

        // Enroll the student in the course
        var enrollment = new StudentCourses
        {
            CourseID = dto.CourseId,
            StudentID = dto.StudentId
        };

        await _studentCoursesRepository.Add(enrollment, cancellationToken);
        await _studentCoursesRepository.SaveChanges(cancellationToken);

        return StudentCourseOperationResult.Success;
    }

    #endregion

    /// <summary>
    /// Applies search filters to the student course query based on the specified enrollment criteria.
    /// </summary>
    /// <remarks>If the CourseTitle property of <paramref name="listDto"/> is not null or empty, the query is
    /// filtered to include only enrollments for courses with a matching title.</remarks>
    /// <param name="query">The initial queryable collection of student course enrollments to filter.</param>
    /// <param name="listDto">An object containing search criteria, such as course title, to apply to the query. Cannot be null.</param>
    /// <returns>An IQueryable<StudentCourses> representing the filtered student course enrollments that match the specified
    /// criteria.</returns>
    private IQueryable<StudentCourses> ApplySearchFilters(IQueryable<StudentCourses> query, ListStudentEnrollmentsDto listDto)
    {
        if (!string.IsNullOrEmpty(listDto.CourseTitle))
        {
            query = query.Where(sc => sc.Course.Title == listDto.CourseTitle);
        }
        return query;
    }
}
