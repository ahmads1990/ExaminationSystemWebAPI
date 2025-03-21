using ExaminationSystem.Application.DTOs.Exams;

namespace ExaminationSystem.Application.Interfaces;

/// <summary>
/// Provides operations for managing exams in the system.
/// </summary>
public interface IExamService
{
    /// <summary>
    /// Retrieves a paginated, sorted, and filtered list of exams.
    /// </summary>
    /// <param name="pageIndex"></param>
    /// <param name="pageSize"></param>
    /// <param name="orderBy"></param>
    /// <param name="sortingDirection"></param>
    /// <param name="title"></param>
    /// <param name="examType"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<(IEnumerable<ExamListDto> Data, int TotalCount)> GetAll(int pageIndex, int pageSize, string? orderBy, SortingDirection sortingDirection, string? title, ExamType? examType, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a single exam by its id.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<ExamDto?> GetByID(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new exam.
    /// </summary>
    /// <param name="examDto"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<ExamDto> Add(AddExamDto examDto, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing exam.
    /// </summary>
    /// <param name="examDto"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<ExamDto?> Update(UpdateExamDto examDto, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes the specified exams
    /// </summary>
    /// <param name="idsToDelete"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<IEnumerable<int>> Delete(List<int> idsToDelete, CancellationToken cancellationToken = default);

    /// <summary>
    /// Persists changes made in the service to the underlying data store.
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task SaveChanges(CancellationToken cancellationToken = default);
}

