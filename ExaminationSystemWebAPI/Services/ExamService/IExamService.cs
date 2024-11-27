using ExaminationSystemWebAPI.Models;

namespace ExaminationSystemWebAPI.Services.ExamService;

public interface IExamService
{
    IQueryable<Exam> GetAll();
    Task<Exam?> GetByID(string id);
    void AddExam(Exam exam);
    void UpdateExam(Exam exam);
    void UpdateExamType(Exam exam);
    void UpdateMaxDuration(Exam exam);
    void UpdateTotalGrade(Exam exam);
    void UpdatePassMark(Exam exam);
    void UpdateIsPublished(Exam exam);
    void UpdateDeadlineDate(Exam exam);
    void Delete(Exam exam);
    void SaveChanges();
}
