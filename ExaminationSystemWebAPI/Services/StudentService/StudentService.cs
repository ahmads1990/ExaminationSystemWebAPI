using ExaminationSystemWebAPI.Data.GenericRepo;
using ExaminationSystemWebAPI.Models.Users;

namespace ExaminationSystemWebAPI.Services.StudentService;

public class StudentService : IStudentService
{
    private readonly IRepository<Student> _studentRepo;

    public StudentService(IRepository<Student> studentRepo)
    {
        _studentRepo = studentRepo;
    }

    public bool StudentExistsByID(string userID)
    {
        return _studentRepo.CheckExistsByID(userID);
    }

    public void AddStudent(Student student)
    {
        _studentRepo.Add(student);
    }
}
