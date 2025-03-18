using ExaminationSystem.Application.DTOs.Questions;

namespace ExaminationSystem.Application.Interfaces;

/// <summary>
/// Provides operations for managing questions in the system.
/// </summary>
public interface IQuestionService
{
    Task<(IEnumerable<QuestionDto> Data, int TotalCount)> GetAll(int pageIndex, int pageSize, int? examId, string? orderBy, SortingDirection sortingDirection, string? body, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a single question by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the question.</param>
    /// <param name="cancellationToken">Optional cancellation token for user to cancel the request</param>
    /// <returns>A <see cref="QuestionDto"/> if found; otherwise, null.</returns>
    Task<QuestionDto?> GetByID(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new question to a exam.
    /// </summary>
    /// <param name="questionDto">The data for the new question.</param>
    /// <param name="cancellationToken">Optional cancellation token for user to cancel the request</param>
    /// <returns>The added <see cref="QuestionDto"/>.</returns>
    Task<QuestionDto> Add(AddQuestionDto questionDto, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing question.
    /// </summary>
    /// <param name="questionDto">The updated question data.</param>
    /// <param name="cancellationToken">Optional cancellation token for user to cancel the request</param>
    /// <returns>The updated <see cref="QuestionDto"/> if successful; otherwise, null.</returns>
    Task<QuestionDto?> Update(UpdateQuestionDto questionDto, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes the specified questions if they are not linked to any exams.
    /// </summary>
    /// <param name="idsToDelete">A list of question IDs to delete.</param>
    /// <param name="cancellationToken">Optional cancellation token for user to cancel the request</param>
    /// <returns>A list of question IDs that could not be deleted (e.g., linked to exams).</returns>
    Task<IEnumerable<int>> Delete(List<int> idsToDelete, CancellationToken cancellationToken = default);

    /// <summary>
    /// Persists changes made in the service to the underlying data store.
    /// </summary>
    /// <param name="cancellationToken">Optional cancellation token for user to cancel the request</param>
    Task SaveChanges(CancellationToken cancellationToken = default);
}
