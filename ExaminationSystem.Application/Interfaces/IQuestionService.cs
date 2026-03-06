using ExaminationSystem.Application.DTOs.Questions;

namespace ExaminationSystem.Application.Interfaces;

/// <summary>
/// Provides operations for managing questions in the system.
/// </summary>
public interface IQuestionService
{
    /// <summary>
    /// Retrieves a paginated, sorted, and filtered list of questions.
    /// </summary>
    /// <param name="listDto">The search, pagination, and sorting criteria.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A tuple containing the list of questions and the total count.</returns>
    Task<(IEnumerable<QuestionDto> Data, int TotalCount)> GetAll(ListQuestionsDto listDto, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a single question by its identifier.
    /// </summary>
    /// <param name="id">The question identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The question details if found, otherwise null.</returns>
    Task<QuestionDto?> GetByID(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new question.
    /// </summary>
    /// <param name="questionDto">The data for the new question.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The operation result.</returns>
    Task<QuestionOperationResult> Add(AddQuestionDto questionDto, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing question.
    /// </summary>
    /// <param name="questionDto">The updated question data.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The operation result.</returns>
    Task<QuestionOperationResult> Update(UpdateQuestionDto questionDto, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes the specified questions.
    /// </summary>
    /// <param name="idsToDelete">List of question IDs to delete.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A collection of IDs that could not be deleted.</returns>
    Task<IEnumerable<int>> Delete(List<int> idsToDelete, CancellationToken cancellationToken = default);

    /// <summary>
    /// Persists changes made in the service to the underlying data store.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task SaveChanges(CancellationToken cancellationToken = default);
}
