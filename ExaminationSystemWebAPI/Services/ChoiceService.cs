using ExaminationSystemWebAPI.Data.GenericRepo;
using ExaminationSystemWebAPI.Models;
using ExaminationSystemWebAPI.Services.Interfaces;

namespace ExaminationSystemWebAPI.Services;

public class ChoiceService : IChoiceService
{
    private readonly IRepository<Choice> _choiceRepo;

    public ChoiceService(IRepository<Choice> choiceRepo)
    {
        _choiceRepo = choiceRepo;
    }

    public IQueryable<Choice> GetAll()
    {
        return _choiceRepo.GetAll();
    }

    public async Task<Choice?> GetByID(string id)
    {
       return await _choiceRepo.GetByID(id);
    }

    public void Add(Choice choice)
    {
        _choiceRepo.Add(choice);
    }

    public void Delete(Choice choice)
    {
        _choiceRepo.Delete(choice);
    }
}
