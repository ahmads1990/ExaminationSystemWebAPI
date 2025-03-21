using ExaminationSystem.Application.DTOs.Exams;

namespace ExaminationSystem.Application.Interfaces;

public interface IExamService
{
    Task<(IEnumerable<ExamListDto> Data, int TotalCount)> GetAll(int pageIndex, int pageSize, string? orderBy, SortingDirection sortingDirection, string? title, ExamType? examType, CancellationToken cancellationToken = default);
    Task<ExamDto?> GetByID(int id, CancellationToken cancellationToken = default);
    Task<ExamDto> Add(AddExamDto examDto, CancellationToken cancellationToken = default);
    Task<ExamDto?> Update(UpdateExamDto examDto, CancellationToken cancellationToken = default);
    Task<IEnumerable<int>> Delete(List<int> idsToDelete, CancellationToken cancellationToken = default);
    Task SaveChanges(CancellationToken cancellationToken = default);
}

