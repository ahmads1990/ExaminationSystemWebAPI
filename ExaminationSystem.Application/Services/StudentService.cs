using ExaminationSystem.Application.DTOs.Student;
using ExaminationSystem.Application.Interfaces;
using ExaminationSystem.Domain.Entities;
using ExaminationSystem.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace ExaminationSystem.Application.Services;

public class StudentService : IStudentService
{
    #region Fields

    private readonly IRepository<Student> _studentsRepo;
    private readonly ILogger<StudentService> _logger;

    #endregion

    #region Constructors

    public StudentService(IRepository<Student> studentsRepo, ILogger<StudentService> logger)
    {
        _studentsRepo = studentsRepo;
        _logger = logger;
    }

    #endregion

    #region Public Methods

    /// <inheritdoc />
    public async Task<UserOperationResult> AddAsync(AddStudentDto studentDto, CancellationToken cancellationToken = default)
    {
        // Validate the required fields
        if (studentDto.ID <= 0)
        {
            _logger.LogWarning("Failed to add student: {Reason}", UserOperationResult.InvalidUserId);
            return UserOperationResult.InvalidUserId;
        }

        var student = studentDto.Adapt<Student>();

        await _studentsRepo.Add(student, cancellationToken);
        await _studentsRepo.SaveChanges(cancellationToken);

        _logger.LogInformation("Student {StudentId} added successfully", student.ID);

        return UserOperationResult.Success;
    }

    #endregion
}
