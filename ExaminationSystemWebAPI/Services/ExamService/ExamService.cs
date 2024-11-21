using ExaminationSystemWebAPI.Data.GenericRepo;
using ExaminationSystemWebAPI.Models;

namespace ExaminationSystemWebAPI.Services.ExamService;

public class ExamService : IExamService
{
    private readonly IRepository<Exam> _examRepo;

    public ExamService(IRepository<Exam> examRepo)
    {
        _examRepo = examRepo;
    }

    public IQueryable<Exam> GetAll()
    {
        return _examRepo.GetAll();
    }

    public async Task<Exam?> GetByID(string id)
    {
        return await _examRepo.GetByID(id);
    }

    public void Add(Exam exam)
    {
        _examRepo.Add(exam);
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
