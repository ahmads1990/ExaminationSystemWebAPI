using ExaminationSystemWebAPI.Data.GenericRepo;
using ExaminationSystemWebAPI.Models;
using ExaminationSystemWebAPI.Models.Joins;
using ExaminationSystemWebAPI.Services.InstructorService;
using ExaminationSystemWebAPI.Services.StudentService;
using ExaminationSystemWebAPI.ViewModels.Course;
using Mapster;

namespace ExaminationSystemWebAPI.Services.CourseService;

public class CourseService : ICourseService
{
    private readonly IRepository<StudentCourses> _studentCoursesRepo;
    private readonly IRepository<Course> _courseRepo;
    private readonly IInstructorService _instructorService;
    private readonly IStudentService _studentService;

    public CourseService(IRepository<Course> courseRepo, IInstructorService instructorService, IStudentService studentService, IRepository<StudentCourses> studentCoursesRepo)
    {
        _courseRepo = courseRepo;
        _instructorService = instructorService;
        _studentService = studentService;
        _studentCoursesRepo = studentCoursesRepo;
    }

    public IQueryable GetAll()
    {
        return _courseRepo.GetAll();
    }

    public bool CourseExistsByID(string id)
    {
        return _courseRepo.CheckExistsByID(id);
    }

    public void AddCourse(Course course)
    {
        var instructorExists = _instructorService.InstructorExistsByID(course.InstructorID);

        if (!instructorExists)
            throw new Exception("Instructor does not exist");

        _courseRepo.Add(course);
    }

    public void AssignStudent(AssignStudentToCourseViewModel viewModel)
    {
        if (!_courseRepo.CheckExistsByID(viewModel.CourseID))
            throw new InvalidOperationException($"Course with ID {viewModel.CourseID} does not exist.");

        if (!_studentService.StudentExistsByID(viewModel.StudentID))
            throw new InvalidOperationException($"Student with ID {viewModel.StudentID} does not exist.");

        var studentCourse = viewModel.Adapt<StudentCourses>();

        _studentCoursesRepo.Add(studentCourse);
    }
}
