using ExaminationSystemWebAPI.Models;
using ExaminationSystemWebAPI.ViewModels.Course;

namespace ExaminationSystemWebAPI.Services.CourseService;

public interface ICourseService
{
    IQueryable GetAll();
    void AddCourse(Course course);
}
