using ExaminationSystemWebAPI.Data.GenericRepo;
using ExaminationSystemWebAPI.Models;

namespace ExaminationSystemWebAPI.Services.QuestionService;

public class QuestionService : IQuestionService
{
    private readonly IRepository<Question> _questionRepo;

    public QuestionService(IRepository<Question> questionRepo)
    {
        _questionRepo = questionRepo;
    }

    public IQueryable<Question> GetAll()
    {
        return _questionRepo.GetAll();
    }

    public async Task<Question?> GetByID(string id)
    {
        return await _questionRepo.GetByID(id);
    }

    public void Add(Question question)
    {
        _questionRepo.Add(question);
    }

    public void Delete(Question question)
    {
        _questionRepo.Delete(question);
    }

    public void SaveChanges()
    {
        _questionRepo.SaveChanges();
    }
}
