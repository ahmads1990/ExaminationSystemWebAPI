using ExaminationSystemWebAPI.Data.GenericRepo;
using ExaminationSystemWebAPI.Models;
using ExaminationSystemWebAPI.Models.Users;
using ExaminationSystemWebAPI.ViewModels.Instructor;

namespace ExaminationSystemWebAPI.Services.InstructorService;

public class InstructorService : IInstructorService
{
    private readonly IRepository<Instructor> _instructorRepo;

    public InstructorService(IRepository<Instructor> instructorRepo)
    {
        _instructorRepo = instructorRepo;
    }
    public bool InstructorExistsByID(string userID)
    {
        return _instructorRepo.CheckExistsByID(userID);
    }
    
    public void AddInstructor(Instructor instructor)
    {
        _instructorRepo.Add(instructor);
    }
}
