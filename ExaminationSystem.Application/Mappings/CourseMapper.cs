using ExaminationSystem.Application.DTOs.Courses;
using ExaminationSystem.Domain.Entities;

namespace ExaminationSystem.Application.Mappings;

/// <summary>
/// Configures mapping rules for Course entities.
/// </summary>
public class CourseMapper : IRegister
{
    #region Public Methods

    /// <summary>
    /// Registers mapping configurations for Course entities.
    /// </summary>
    /// <param name="config">The type adapter configuration.</param>
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<Course, CourseDto>()
            .Map(dest => dest.InstructorName, src => src.Instructor != null && src.Instructor.AppUser != null
                ? src.Instructor.AppUser.Name
                : null);
    }

    #endregion
}
