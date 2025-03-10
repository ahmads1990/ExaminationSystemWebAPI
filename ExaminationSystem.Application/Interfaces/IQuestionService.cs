using ExaminationSystem.Application.DTOs.Questions;

namespace ExaminationSystem.Application.Interfaces;

/// <summary>
/// Provides operations for managing questions in the system.
/// </summary>
public interface IQuestionService
{
    /// <summary>
    /// Retrieves a paginated list of questions.
    /// </summary>
    /// <param name="start">The page index (zero-based).</param>
    /// <param name="length">The number of records to retrieve per page.</param>
    /// <returns>A collection of <see cref="QuestionDto"/>.</returns>
    Task<IEnumerable<QuestionDto>> GetAll(int start, int length);

    /// <summary>
    /// Retrieves a single question by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the question.</param>
    /// <returns>A <see cref="QuestionDto"/> if found; otherwise, null.</returns>
    Task<QuestionDto?> GetByID(int id);

    /// <summary>
    /// Adds a new question to the system.
    /// </summary>
    /// <param name="questionDto">The data for the new question.</param>
    /// <returns>The added <see cref="QuestionDto"/>.</returns>
    Task<QuestionDto> Add(AddQuestionDto questionDto);

    /// <summary>
    /// Updates an existing question.
    /// </summary>
    /// <param name="questionDto">The updated question data.</param>
    /// <returns>The updated <see cref="QuestionDto"/> if successful; otherwise, null.</returns>
    Task<QuestionDto?> Update(UpdateQuestionDto questionDto);

    /// <summary>
    /// Deletes the specified questions if they are not linked to any exams.
    /// </summary>
    /// <param name="idsToDelete">A list of question IDs to delete.</param>
    /// <returns>A list of question IDs that could not be deleted (e.g., linked to exams).</returns>
    Task<IEnumerable<int>> Delete(List<int> idsToDelete);

    /// <summary>
    /// Persists changes made in the service to the underlying data store.
    /// </summary>
    Task SaveChanges();
}
