using ExaminationSystem.Application.DTOs.Instructor;
using ExaminationSystem.Application.DTOs.Student;

namespace ExaminationSystem.Application.Interfaces;

public interface IStudentService
{
    Task<(UserOperationResult result, int Id)> AddAsync(AddStudentDto studentDto, CancellationToken cancellationToken = default);
}
