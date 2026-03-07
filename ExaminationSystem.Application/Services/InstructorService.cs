using ExaminationSystem.Application.DTOs.Instructor;
using ExaminationSystem.Application.Interfaces;
using ExaminationSystem.Domain.Entities;
using ExaminationSystem.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace ExaminationSystem.Application.Services;

public class InstructorService : IInstructorService
{
    #region Fields

    private readonly IRepository<Instructor> _instructorRepository;
    private readonly ILogger<InstructorService> _logger;

    #endregion

    #region Constructors

    public InstructorService(IRepository<Instructor> instructorRepository, ILogger<InstructorService> logger)
    {
        _instructorRepository = instructorRepository;
        _logger = logger;
    }

    #endregion

    #region Public Methods

    /// <inheritdoc />
    public async Task<UserOperationResult> AddAsync(AddInstructorDto instructorDto, CancellationToken cancellationToken = default)
    {
        // Validate the required fields
        if (instructorDto.ID <= 0)
        {
            _logger.LogWarning("Failed to add instructor: {Reason}", UserOperationResult.InvalidUserId);
            return UserOperationResult.InvalidUserId;
        }

        var instructor = instructorDto.Adapt<Instructor>();

        await _instructorRepository.Add(instructor, cancellationToken);
        await _instructorRepository.SaveChanges(cancellationToken);

        _logger.LogInformation("Instructor {InstructorId} added successfully", instructor.ID);

        return UserOperationResult.Success;
    }

    #endregion
}

