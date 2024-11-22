using ExaminationSystemWebAPI.Models;

namespace ExaminationSystemWebAPI.Services.QuestionService;

public interface IQuestionService
{
    IQueryable<Question> GetAll();
    Task<Question?> GetByID(string id);
    void Add(Question question);
    IEnumerable<Question> AddMultipleQuestions(IEnumerable<Question> questions);
    void UpdateQuestion(Question question);
    void UpdateLevel(Question question);
    void UpdateBody(Question question);
    void UpdateScore(Question question);
    void Delete(Question question);
    void SaveChanges();
}
