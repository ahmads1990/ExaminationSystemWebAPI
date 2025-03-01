using ExaminationSystem.Application.DTOs.Questions;
using ExaminationSystem.Domain.Entities;

namespace ExaminationSystem.Application.Interfaces;

public interface IQuestionService
{
    //IQueryable<Question> GetAll();
    //Task<Question?> GetByID(int id);
    Task<QuestionDto> Add(AddQuestionDto questionDto);
    //Task<IEnumerable<Question>> AddRange(AddQuestionsRequest request);
    //void UpdateQuestion(UpdateQuestionRequest request);
    //void Delete(DeleteQuestionsRequest request);
    Task SaveChanges();
}
