using ExaminationSystem.Application.DTOs.Exams;
using ExaminationSystem.Domain.Entities;

namespace ExaminationSystem.Application.Mappings;

public class ExamMapper : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<Exam, ExamDto>()
            .Map(dest => dest.Course, src => src.Course.Title);

        config.NewConfig<Exam, ExamListDto>()
            .Map(dest => dest.Course, src => src.Course.Title);
    }
}
