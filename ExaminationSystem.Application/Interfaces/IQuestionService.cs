using ExaminationSystem.Application.DTOs.Questions;

namespace ExaminationSystem.Application.Interfaces;

public interface IQuestionService
{
    Task<IEnumerable<QuestionDto>> GetAll(int start, int length);
    Task<QuestionDto?> GetByID(int id);
    Task<QuestionDto> Add(AddQuestionDto questionDto);
    Task<QuestionDto?> Update(UpdateQuestionDto questionDto);
    Task<IEnumerable<int>> Delete(List<int> idsToDelete);
    Task SaveChanges();
}
