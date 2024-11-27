using ExaminationSystemWebAPI.Data.GenericRepo;
using ExaminationSystemWebAPI.Models;
using ExaminationSystemWebAPI.Services.InstructorService;
using ExaminationSystemWebAPI.ViewModels.Course;

namespace ExaminationSystemWebAPI.Services.CourseService;

public class CourseService : ICourseService
{
    private readonly IRepository<Course> _courseRepo;
    private readonly IInstructorService _instructorService;

    public CourseService(IRepository<Course> courseRepo, IInstructorService instructorService)
    {
        _courseRepo = courseRepo;
        _instructorService = instructorService;
    }
    public IQueryable GetAll()
    {
        return _courseRepo.GetAll();
    }

    public void AddCourse(Course course)
    {
        var instructorExists = _instructorService.InstructorExistsByID(course.InstructorID);

        if (!instructorExists)
            throw new Exception("Instructor does not exist");

        _courseRepo.Add(course);
    }
}
