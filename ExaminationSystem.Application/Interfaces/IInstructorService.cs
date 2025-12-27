using ExaminationSystem.Application.DTOs.Instructor;

namespace ExaminationSystem.Application.Interfaces;

public interface IInstructorService
{
    Task<UserOperationResult> AddAsync(AddInstructorDto userDto, CancellationToken cancellationToken = default);
}

