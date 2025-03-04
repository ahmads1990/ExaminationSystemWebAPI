using ExaminationSystem.Application.DTOs.Questions;
using ExaminationSystem.Application.Interfaces;
using ExaminationSystem.Domain.Entities;
using ExaminationSystem.Domain.Interfaces;
using MapsterMapper;
namespace ExaminationSystem.Application.Services;

public class QuestionService : IQuestionService
{
    private readonly IRepository<Question> _questionRepository;
    private readonly IRepository<Choice> _choiceRepository;
    private readonly IMapper _mapper;

    public QuestionService(IRepository<Question> questionRepository, IRepository<Choice> choiceRepository, IMapper mapper)
    {
        _questionRepository = questionRepository;
        _choiceRepository = choiceRepository;
        _mapper = mapper;
    }

    public IQueryable<Question> GetAll()
    {
        return _questionRepository.GetAll();
    }

    public async Task<QuestionDto> Add(AddQuestionDto questionDto)
    {
        var question = _mapper.Map<Question>(questionDto);

        await _choiceRepository.AddRange(question.Choices);

        await _questionRepository.Add(question);
        await SaveChanges();

        return _mapper.Map<QuestionDto>(questionDto);
    }

    //public Task<Question?> GetByID(int id)
    //{
    //    throw new NotImplementedException();
    //}

    //public Task<IEnumerable<Question>> AddRange(AddQuestionsRequest request)
    //{
    //    throw new NotImplementedException();
    //}

    //public void UpdateQuestion(UpdateQuestionRequest request)
    //{
    //    throw new NotImplementedException();
    //}

    //public void Delete(DeleteQuestionsRequest request)
    //{
    //    throw new NotImplementedException();
    //}

    public async Task SaveChanges()
    {
        await _questionRepository.SaveChanges();
    }
}
