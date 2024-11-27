using ExaminationSystemWebAPI.Models;
using ExaminationSystemWebAPI.Services.CourseService;
using ExaminationSystemWebAPI.ViewModels.Course;
using Mapster;
using Microsoft.AspNetCore.Mvc;

namespace ExaminationSystemWebAPI.Controllers;

[ApiController]
[Route("[controller]/[action]")]
public class CourseController : ControllerBase
{
    private readonly ICourseService _courseService;

    public CourseController(ICourseService courseService)
    {
        _courseService = courseService;
    }

    [HttpGet]
    public IActionResult GetAll()
    {
        var courses = _courseService
            .GetAll();

        return Ok(courses);
    }

    [HttpPost]
    public IActionResult AddCourse(AddCourseViewModel viewModel)
    {
        var course = viewModel.Adapt<Course>();
        _courseService.AddCourse(course);

        return Ok();
    }
}
