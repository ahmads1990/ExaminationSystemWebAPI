using ExaminationSystem.Application.DTOs.Instructor;

namespace ExaminationSystem.Application.Interfaces;

public interface IInstructorService
{
    Task<(AddInstructorResult result, int Id)> Add(AddInstructorDto userDto, CancellationToken cancellationToken = default);
}

