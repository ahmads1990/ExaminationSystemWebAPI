using ExaminationSystem.Application.DTOs.Instructor;

namespace ExaminationSystem.Application.Interfaces;

public interface IInstructorService
{
    Task<(UserOperationResult result, int Id)> AddAsync(AddInstructorDto userDto, CancellationToken cancellationToken = default);
}

