using ExaminationSystem.Application.DTOs.Instructor;
using ExaminationSystem.Application.Interfaces;
using ExaminationSystem.Domain.Entities;
using ExaminationSystem.Domain.Interfaces;

namespace ExaminationSystem.Application.Services;

public class InstructorService : IInstructorService
{
    private readonly IRepository<Instructor> _instructorRepository;
    private readonly IRepository<AppUser> _userRepository;

    public InstructorService(IRepository<Instructor> instructorRepository, IRepository<AppUser> userRepository)
    {
        _instructorRepository = instructorRepository;
        _userRepository = userRepository;
    }

    public async Task<(AddInstructorResult result, int Id)> Add(AddInstructorDto instructorDto, CancellationToken cancellationToken = default)
    {
        // Validate the required fields
        if (instructorDto.AppUserId <= 0)
            return (AddInstructorResult.InvalidUserId, 0);

        // Check if user exists
        var userExists = await _userRepository.CheckExistsByID(instructorDto.AppUserId);
        if (!userExists)
            return (AddInstructorResult.UserNotFound, 0);

        // Check if user is already an instructor
        var isAlreadyInstructor = await _instructorRepository.CheckExistsByID(instructorDto.AppUserId);
        if (isAlreadyInstructor)
            return (AddInstructorResult.AlreadyInstructor, 0);

        var instructor = instructorDto.Adapt<Instructor>();

        await _instructorRepository.Add(instructor, cancellationToken);
        await _instructorRepository.SaveChanges(cancellationToken);

        return (AddInstructorResult.Success, instructor.ID);
    }
}

