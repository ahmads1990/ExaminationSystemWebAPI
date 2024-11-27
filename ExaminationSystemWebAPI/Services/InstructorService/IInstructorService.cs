using ExaminationSystemWebAPI.Models.Users;

namespace ExaminationSystemWebAPI.Services.InstructorService;

public interface IInstructorService
{
    bool InstructorExistsByID(string userID);
    void AddInstructor(Instructor instructor);
}
