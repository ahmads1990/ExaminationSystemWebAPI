using ExaminationSystem.Application.DTOs.Student;

namespace ExaminationSystem.Application.Interfaces;

public interface IStudentService
{
    Task<UserOperationResult> AddAsync(AddStudentDto studentDto, CancellationToken cancellationToken = default);
}
