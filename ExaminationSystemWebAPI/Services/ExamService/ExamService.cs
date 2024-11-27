using ExaminationSystemWebAPI.Data.GenericRepo;
using ExaminationSystemWebAPI.Models;
using ExaminationSystemWebAPI.Services.CourseService;
using ExaminationSystemWebAPI.Services.QuestionService;

namespace ExaminationSystemWebAPI.Services.ExamService;

public class ExamService : IExamService
{
    private readonly IRepository<Exam> _examRepo;
    private readonly IQuestionService _questionService;
    private readonly ICourseService _courseService;

    public ExamService(IRepository<Exam> examRepo, IQuestionService questionService, ICourseService courseService)
    {
        _examRepo = examRepo;
        _questionService = questionService;
        _courseService = courseService;
    }

    public IQueryable<Exam> GetAll()
    {
        return _examRepo.GetAll();
    }

    public async Task<Exam?> GetByID(string id)
    {
        return await _examRepo.GetByID(id);
    }
    public void AddExam(Exam exam)
    {
        // Check total grade
        if (exam.Questions.Count > 0)
        {
            var questionsScore = exam.Questions.Sum(q => q.Score);
            if (questionsScore != exam.TotalGrade)
                throw new Exception($"Wrong questions Score :{questionsScore} is not equal to exam Total Grade {exam.TotalGrade}");
        }

        // Check course exists
        var courseExists = _courseService.CourseExistsByID(exam.CourseID);

        if (!courseExists)
        {
            throw new Exception($"Course does not exist");
        }

        _examRepo.Add(exam);
    }

    public void AddFullExam(Exam exam)
    {
        exam.Questions = (ICollection<Question>)_questionService.AddMultipleQuestions(exam.Questions);

        AddExam(exam);
    }

    public void UpdateExam(Exam exam)
    {

    }

    public void UpdateExamType(Exam exam)
    {
        _examRepo.SaveInclude(exam, nameof(Exam.ExamType));
    }

    public void UpdateMaxDuration(Exam exam)
    {
        _examRepo.SaveInclude(exam, nameof(Exam.MaxDuration));
    }

    public void UpdateTotalGrade(Exam exam)
    {
        _examRepo.SaveInclude(exam, nameof(Exam.TotalGrade));
    }

    public void UpdatePassMark(Exam exam)
    {
        _examRepo.SaveInclude(exam, nameof(Exam.PassMark));
    }

    public void UpdateIsPublished(Exam exam)
    {
        _examRepo.SaveInclude(exam, nameof(Exam.IsPublished));
    }

    public void UpdateDeadlineDate(Exam exam)
    {
        _examRepo.SaveInclude(exam, nameof(Exam.DeadlineDate));
    }

    public void Delete(Exam exam)
    {
        _examRepo.Delete(exam);
    }

    public void SaveChanges()
    {
        _examRepo.SaveChanges();
    }
}
