using ExaminationSystem.Application.DTOs.Exams;
using ExaminationSystem.Domain.Entities;

namespace ExaminationSystem.Application.Mappings;

/// <summary>
/// Configures mapping rules for Exam entities.
/// </summary>
public class ExamMapper : IRegister
{
    #region Public Methods

    /// <summary>
    /// Registers mapping configurations for Exam entities.
    /// </summary>
    /// <param name="config">The type adapter configuration.</param>
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<Exam, ExamDto>()
            .Map(dest => dest.Course, src => src.Course.Title);

        config.NewConfig<Exam, ExamListDto>()
            .Map(dest => dest.Course, src => src.Course.Title);
    }

    #endregion
}
