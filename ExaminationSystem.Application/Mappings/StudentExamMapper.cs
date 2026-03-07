using ExaminationSystem.Application.DTOs.StudentExams;
using ExaminationSystem.Domain.Entities;

namespace ExaminationSystem.Application.Mappings;

/// <summary>
/// Configures mapping rules for Student Exam entities.
/// </summary>
public class StudentExamMapper : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<ExamAttempt, AttemptSummaryDto>()
            .Map(dest => dest.CourseName, src => src.Exam != null && src.Exam.Course != null ? src.Exam.Course.Title : string.Empty)
            .Map(dest => dest.ExamTitle, src => src.Exam != null ? src.Exam.Title : string.Empty)
            .Map(dest => dest.ExamType, src => src.Exam != null ? src.Exam.ExamType : ExamType.Quiz)
            .Map(dest => dest.Grade, src => src.Score ?? 0)
            .Map(dest => dest.MaxGrade, src => src.Exam != null ? src.Exam.TotalGrade : 0)
            .Map(dest => dest.Status, src => src.ExamAttemptStatus)
            .Map(dest => dest.CompletionTime, src => src.EndTime > src.StartTime
                ? $"{(src.EndTime - src.StartTime).TotalMinutes:F1} min"
                : "-")
            .Map(dest => dest.CreateDate, src => src.StartTime);

        config.NewConfig<ExamAttempt, AttemptResultDto>()
            .Map(dest => dest.CurrentGrade, src => src.Score ?? 0)
            .Map(dest => dest.MaxGrade, src => src.Exam != null ? src.Exam.TotalGrade : 0)
            .Map(dest => dest.CompletionTime, src => src.EndTime > src.StartTime
                ? $"{(src.EndTime - src.StartTime).TotalMinutes:F1} minutes"
                : "0 minutes");

        config.NewConfig<Exam, AvailableExamDto>()
            .Map(dest => dest.ExamId, src => src.ID)
            .Map(dest => dest.CourseName, src => src.Course != null ? src.Course.Title : string.Empty)
            .Ignore(dest => dest.AttemptsTaken);
    }
}
