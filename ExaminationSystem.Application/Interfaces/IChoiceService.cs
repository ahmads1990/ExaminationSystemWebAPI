using ExaminationSystem.Application.DTOs.Choices;
using ExaminationSystem.Domain.Entities;

namespace ExaminationSystem.Application.Interfaces;

public interface IChoiceService
{
    //IQueryable<Choice> GetAll();
    //Task<Choice?> GetByID(int id);
    Task<Choice> Add(AddChoiceDto choiceDto);
    Task<IEnumerable<Choice>> AddRange(IEnumerable<AddChoiceDto> choiceDtos);
    //void UpdateTextBody(Choice choice);
    //void Delete(Choice choice);
    //void SaveChanges();
}
