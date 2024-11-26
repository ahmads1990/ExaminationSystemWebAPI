using ExaminationSystemWebAPI.Data.GenericRepo;
using ExaminationSystemWebAPI.Models.Users;
using ExaminationSystemWebAPI.ViewModels.Student;

namespace ExaminationSystemWebAPI.Services.StudentService;

public class StudentService : IStudentService
{
    private readonly IRepository<Student> _studentRepo;

    public StudentService(IRepository<Student> studentRepo)
    {
        _studentRepo = studentRepo;
    }
    public void AddStudent(Student student)
    {
        _studentRepo.Add(student);
    }
}
