using ExaminationSystemWebAPI.Models.Users;
using ExaminationSystemWebAPI.ViewModels.Instructor;

namespace ExaminationSystemWebAPI.Services.InstructorService;

public interface IInstructorService
{
    void AddInstructor(Instructor instructor);
}
