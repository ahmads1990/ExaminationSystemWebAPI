using ExaminationSystemWebAPI.Models;

namespace ExaminationSystemWebAPI.Services.QuestionService;

public interface IQuestionService
{
    IQueryable<Question> GetAll();
    Task<Question?> GetByID(string id);
    void Add(Question question);
    void Delete(Question question);
    void SaveChanges();
}
