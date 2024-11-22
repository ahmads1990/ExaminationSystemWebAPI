using ExaminationSystemWebAPI.Models;

namespace ExaminationSystemWebAPI.Services.ChoiceService;

public interface IChoiceService
{
    IQueryable<Choice> GetAll();
    Task<Choice?> GetByID(string id);
    void Add(Choice choice);
    IEnumerable<Choice> AddMultipleChoices(IEnumerable<Choice> choices);
    void UpdateTextBody(Choice choice);
    void Delete(Choice choice);
    void SaveChanges();
}
