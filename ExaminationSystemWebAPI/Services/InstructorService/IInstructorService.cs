using ExaminationSystemWebAPI.Models.Users;
using ExaminationSystemWebAPI.ViewModels.Instructor;

namespace ExaminationSystemWebAPI.Services.InstructorService;

public interface IInstructorService
{
    bool InstructorExistsByID(string userID);
    void AddInstructor(Instructor instructor);
}
