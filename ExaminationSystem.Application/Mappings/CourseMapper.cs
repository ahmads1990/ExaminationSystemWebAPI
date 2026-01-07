using ExaminationSystem.Application.DTOs.Courses;
using ExaminationSystem.Domain.Entities;

namespace ExaminationSystem.Application.Mappings;

public class CourseMapper : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<Course, CourseDto>()
            .Map(dest => dest.InstructorName, src => src.Instructor != null && src.Instructor.AppUser != null
                ? src.Instructor.AppUser.Name
                : null);
    }
}
