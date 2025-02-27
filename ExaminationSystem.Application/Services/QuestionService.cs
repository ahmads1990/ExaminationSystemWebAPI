using ExaminationSystem.Application.DTOs.Questions;
using ExaminationSystem.Application.Interfaces;
using ExaminationSystem.Domain.Entities;
using ExaminationSystem.Domain.Interfaces;
using MapsterMapper;
namespace ExaminationSystem.Application.Services;

public class QuestionService : IQuestionService
{
    private readonly IRepository<Question> _repository;
    private readonly IMapper _mapper;

    public QuestionService(IRepository<Question> repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public IQueryable<Question> GetAll()
    {
        throw new NotImplementedException();
    }

    public Task<Question> Add(AddQuestionDto request)
    {
        var question = _mapper.Map<Question>(request);
    }

    public Task<Question?> GetByID(int id)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<Question>> AddRange(AddQuestionsRequest request)
    {
        throw new NotImplementedException();
    }

    public void UpdateQuestion(UpdateQuestionRequest request)
    {
        throw new NotImplementedException();
    }

    public void Delete(DeleteQuestionsRequest request)
    {
        throw new NotImplementedException();
    }

    public Task SaveChanges()
    {
        throw new NotImplementedException();
    }
}
