using ExaminationSystem.Application.DTOs.Student;
using ExaminationSystem.Application.Interfaces;
using ExaminationSystem.Domain.Entities;
using ExaminationSystem.Domain.Interfaces;

namespace ExaminationSystem.Application.Services;

public class StudentService : IStudentService
{
    private readonly IRepository<Student> _studentsRepo;

    public StudentService(IRepository<Student> studentsRepo)
    {
        _studentsRepo = studentsRepo;
    }

    #region Public Methods

    public async Task<UserOperationResult> AddAsync(AddStudentDto studentDto, CancellationToken cancellationToken = default)
    {
        // Validate the required fields
        if (studentDto.ID <= 0)
            return UserOperationResult.InvalidUserId;

        var student = studentDto.Adapt<Student>();

        await _studentsRepo.Add(student, cancellationToken);
        await _studentsRepo.SaveChanges(cancellationToken);

        return UserOperationResult.Success;
    }

    #endregion
}
