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
    /// <param name="listDto">The search, pagination, and sorting criteria.</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<(IEnumerable<ExamListDto> Data, int TotalCount)> GetAll(ListExamsDto listDto, CancellationToken cancellationToken = default);

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
    /// <param name="examDto">The data for the new exam.</param>
    /// <param name="cancellationToken"></param>
    /// <returns>
    /// A tuple containing the <see cref="ExamOperationResult"/> indicating the outcome
    /// and the generated ID of the new exam if successful; otherwise, 0.
    /// </returns>
    Task<(ExamOperationResult Result, int Id)> Add(AddExamDto examDto, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing exam's settings.
    /// </summary>
    /// <param name="examDto">The updated exam data.</param>
    /// <param name="cancellationToken"></param>
    /// <returns>An <see cref="ExamOperationResult"/> indicating success or the reason for failure.</returns>
    Task<ExamOperationResult> Update(UpdateExamDto examDto, CancellationToken cancellationToken = default);

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

    /// <summary>
    /// Publishes an exam, making it visible and available to students.
    /// </summary>
    /// <param name="publishExamDto">The publish request containing the exam ID and optional publish date.</param>
    /// <param name="cancellationToken"></param>
    /// <returns>An <see cref="ExamOperationResult"/> indicating success or the reason for failure.</returns>
    /// <remarks>
    /// If <see cref="PublishExamDto.PublishDate"/> is null, the publish date defaults to <see cref="DateTime.UtcNow"/>.
    /// </remarks>
    Task<ExamOperationResult> Publish(PublishExamDto publishExamDto, CancellationToken cancellationToken = default);

    /// <summary>
    /// Unpublishes an exam, reverting it to draft status and clearing its publish date.
    /// </summary>
    /// <param name="id">The ID of the exam to unpublish.</param>
    /// <param name="cancellationToken"></param>
    /// <returns>An <see cref="ExamOperationResult"/> indicating success or the reason for failure.</returns>
    Task<ExamOperationResult> UnPublish(int id, CancellationToken cancellationToken = default);
}

