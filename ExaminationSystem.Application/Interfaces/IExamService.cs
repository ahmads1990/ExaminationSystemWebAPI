using ExaminationSystem.Application.DTOs;
using ExaminationSystem.Application.DTOs.Exams;
using ExaminationSystem.Application.DTOs.StudentExams;

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
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A tuple containing the list of exams and the total count.</returns>
    Task<(IEnumerable<ExamListDto> Data, int TotalCount)> GetAll(ListExamsDto listDto, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a single exam by its identifier.
    /// </summary>
    /// <param name="id">The exam identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The exam details if found, otherwise null.</returns>
    Task<ExamDto?> GetByID(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new exam.
    /// </summary>
    /// <param name="examDto">The data for the new exam.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>
    /// A tuple containing the <see cref="ExamOperationResult"/> indicating the outcome
    /// and the generated ID of the new exam if successful; otherwise, 0.
    /// </returns>
    Task<(ExamOperationResult Result, int Id)> Add(AddExamDto examDto, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing exam's settings.
    /// </summary>
    /// <param name="examDto">The updated exam data.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>An <see cref="ExamOperationResult"/> indicating success or the reason for failure.</returns>
    Task<ExamOperationResult> Update(UpdateExamDto examDto, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes the specified exams.
    /// </summary>
    /// <param name="idsToDelete">List of exam IDs to delete.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A collection of IDs that could not be deleted.</returns>
    Task<IEnumerable<int>> Delete(List<int> idsToDelete, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a single exam by its identifier.
    /// Deletion is blocked when the exam is published or has student submissions.
    /// </summary>
    /// <param name="id">The exam identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>An <see cref="ExamOperationResult"/> indicating success or the reason for failure.</returns>
    Task<ExamOperationResult> DeleteById(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Persists changes made in the service to the underlying data store.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task SaveChanges(CancellationToken cancellationToken = default);

    /// <summary>
    /// Publishes an exam, making it visible and available to students.
    /// </summary>
    /// <param name="publishExamDto">The publish request containing the exam ID and optional publish date.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>An <see cref="ExamOperationResult"/> indicating success or the reason for failure.</returns>
    /// <remarks>
    /// If <see cref="PublishExamDto.PublishDate"/> is null, the publish date defaults to <see cref="DateTime.UtcNow"/>.
    /// </remarks>
    Task<ExamOperationResult> Publish(PublishExamDto publishExamDto, CancellationToken cancellationToken = default);

    /// <summary>
    /// Unpublishes an exam, reverting it to draft status and clearing its publish date.
    /// </summary>
    /// <param name="id">The ID of the exam to unpublish.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>An <see cref="ExamOperationResult"/> indicating success or the reason for failure.</returns>
    Task<ExamOperationResult> UnPublish(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Assigns questions to an exam. Already-assigned or non-existent questions are returned as rejected.
    /// </summary>
    /// <param name="dto">The exam ID and question IDs to assign.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>
    /// A tuple of the exam-level result and a collection of rejected question entries.
    /// An empty rejected list means all questions were assigned successfully.
    /// </returns>
    Task<(ExamOperationResult Result, IEnumerable<RejectedEntityDto> Rejected)> AssignQuestions(AssignQuestionsDto dto, CancellationToken cancellationToken = default);

    /// <summary>
    /// Unassigns questions from an exam. Non-assigned or non-existent questions are returned as rejected.
    /// </summary>
    /// <param name="dto">The exam ID and question IDs to unassign.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>
    /// A tuple of the exam-level result and a collection of rejected question entries.
    /// An empty rejected list means all questions were unassigned successfully.
    /// </returns>
    Task<(ExamOperationResult Result, IEnumerable<RejectedEntityDto> Rejected)> UnassignQuestions(AssignQuestionsDto dto, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves all student submissions for a specific exam.
    /// </summary>
    /// <param name="examId">The exam identifier.</param>
    /// <param name="instructorId">The instructor identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A tuple of the exam operation result and the list of attempt summaries.</returns>
    Task<(ExamOperationResult Result, List<AttemptSummaryDto>? Submissions)> GetExamSubmissions(int examId, int instructorId, CancellationToken cancellationToken = default);
}

