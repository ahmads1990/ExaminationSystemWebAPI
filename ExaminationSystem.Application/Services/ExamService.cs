using ExaminationSystem.Application.DTOs.Exams;
using ExaminationSystem.Application.Interfaces;
using ExaminationSystem.Domain.Entities;
using ExaminationSystem.Domain.Interfaces;

namespace ExaminationSystem.Application.Services;

public class ExamService : IExamService
{
    private readonly IRepository<Exam> _examRepository;
    private readonly IQuestionService _questionService;

    public ExamService(IRepository<Exam> examRepository, IQuestionService questionService)
    {
        _examRepository = examRepository;
        _questionService = questionService;
    }

    public async Task<ExamDto> Add(AddExamDto examDto, CancellationToken cancellationToken = default)
    {
        var exam = examDto.Adapt<Exam>();

        // Temp for now
        exam.CourseID = 1;

        await _examRepository.Add(exam);
        await _examRepository.SaveChanges(cancellationToken);

        _questionService.Add();
    }
}

