using ExaminationSystem.Application.DTOs.Instructor;
using ExaminationSystem.Application.Interfaces;
using ExaminationSystem.Domain.Entities;
using ExaminationSystem.Domain.Interfaces;

namespace ExaminationSystem.Application.Services;

public class InstructorService : IInstructorService
{
    private readonly IRepository<Instructor> _instructorRepository;

    public InstructorService(IRepository<Instructor> instructorRepository)
    {
        _instructorRepository = instructorRepository;
    }

    #region Public Methods

    public async Task<UserOperationResult> AddAsync(AddInstructorDto instructorDto, CancellationToken cancellationToken = default)
    {
        // Validate the required fields
        if (instructorDto.ID <= 0)
            return UserOperationResult.InvalidUserId;

        var instructor = instructorDto.Adapt<Instructor>();

        await _instructorRepository.Add(instructor, cancellationToken);
        await _instructorRepository.SaveChanges(cancellationToken);

        return UserOperationResult.Success;
    }

    #endregion
}

