using ExaminationSystemWebAPI.Models.Users;

namespace ExaminationSystemWebAPI.Services.StudentService;

public interface IStudentService
{
    bool StudentExistsByID(string userID);
    void AddStudent(Student student);
}
