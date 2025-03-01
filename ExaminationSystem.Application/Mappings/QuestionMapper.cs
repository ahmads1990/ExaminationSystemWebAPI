using ExaminationSystem.Application.DTOs.Questions;
using ExaminationSystem.Domain.Entities;
using Mapster;

namespace ExaminationSystem.Application.Mappings;

public class QuestionMapper : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<AddQuestionDto, Question>();
    }
}
