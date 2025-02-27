using ExaminationSystem.Application.DTOs.Requests.Questions;
using ExaminationSystem.Domain.Entities;

namespace ExaminationSystem.Application.Interfaces;

public interface IQuestionService
{
    IQueryable<Question> GetAll();
    Task<Question?> GetByID(int id);
    Task<Question> Add(AddQuestionRequest request);
    Task<IEnumerable<Question>> AddRange(AddQuestionsRequest request);
    void UpdateQuestion(UpdateQuestionRequest request);
    void Delete(DeleteQuestionsRequest request);
    Task SaveChanges();
}
