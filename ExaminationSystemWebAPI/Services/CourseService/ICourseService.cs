using ExaminationSystemWebAPI.Models;
using ExaminationSystemWebAPI.ViewModels.Course;

namespace ExaminationSystemWebAPI.Services.CourseService;

public interface ICourseService
{
    IQueryable GetAll();
    bool CourseExistsByID(string id);
    void AddCourse(Course course);
    void AssignStudent(AssignStudentToCourseViewModel viewModel);
}
