using ExaminationSystemWebAPI.Models;

namespace ExaminationSystemWebAPI.Services.Interfaces;

public interface IChoiceService
{
    IQueryable<Choice> GetAll();
    Task<Choice?> GetByID(string id);
    void Add(Choice choice);
    void Delete(Choice choice);
}
