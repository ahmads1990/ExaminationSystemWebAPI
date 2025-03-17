using ExaminationSystem.Application.DTOs.Exams;

namespace ExaminationSystem.Application.Interfaces;

public interface IExamService
{
    Task<ExamDto> Add(AddExamDto examDto, CancellationToken cancellationToken = default);
}

