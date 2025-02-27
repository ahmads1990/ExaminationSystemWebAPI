using ExaminationSystem.Application.DTOs.Requests.Choices;
using ExaminationSystem.Application.DTOs.Requests.Questions;
using ExaminationSystem.Domain.Entities;
using Mapster;

namespace ExaminationSystem.Application.Mappings;

public class QuestionMapper : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<AddQuestionRequest, Question>()
            .Map(q=>q.ans)
    }
}
