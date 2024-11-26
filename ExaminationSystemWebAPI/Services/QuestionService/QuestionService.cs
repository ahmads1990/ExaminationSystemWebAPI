using ExaminationSystemWebAPI.Data.GenericRepo;
using ExaminationSystemWebAPI.Models;
using ExaminationSystemWebAPI.Services.ChoiceService;
using Microsoft.IdentityModel.Tokens;

namespace ExaminationSystemWebAPI.Services.QuestionService;

public class QuestionService : IQuestionService
{
    private readonly IRepository<Question> _questionRepo;
    private readonly IChoiceService _choiceService;

    public QuestionService(IRepository<Question> questionRepo, IChoiceService choiceService)
    {
        _questionRepo = questionRepo;
        _choiceService = choiceService;
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

    public IEnumerable<Question> AddMultipleQuestions(IEnumerable<Question> questions)
    {
        // Vaildat maybe questions number??

        foreach (var question in questions)
        {
            if (question.Choices.Count() > 4)
                throw new Exception($"Question is allowed to have only 4 or less choices you have {question.Choices.Count()}");

            var choices = _choiceService.AddMultipleChoices(question.Choices);
            question.Choices = choices;
        }
        return _questionRepo.AddRange(questions);
    }

    public void UpdateQuestion(Question question)
    {
        if (!question.Choices.IsNullOrEmpty())
        {
            foreach (var choice in question.Choices)
            {
                _choiceService.UpdateTextBody(choice);
            }
        }

        _questionRepo.SaveInclude(question, 
            nameof(Question.QuestionLevel),
            nameof(Question.TextBody),
            nameof(Question.Score)
            );
    }
    public void UpdateLevel(Question question)
    {
        _questionRepo.SaveInclude(question, nameof(Question.QuestionLevel));
    }
    public void UpdateBody(Question question)
    {
        _questionRepo.SaveInclude(question, nameof(Question.TextBody));
    }
    public void UpdateScore(Question question)
    {
        _questionRepo.SaveInclude(question, nameof(Question.Score));
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
