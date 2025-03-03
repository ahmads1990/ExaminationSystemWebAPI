using ExaminationSystem.Application.DTOs.Choices;
using ExaminationSystem.Application.Interfaces;
using ExaminationSystem.Domain.Entities;
using ExaminationSystem.Domain.Interfaces;
using MapsterMapper;

namespace ExaminationSystem.Application.Services;

public class ChoiceService : IChoiceService
{
    private readonly IRepository<Choice> _repository;
    private readonly IMapper _mapper;

    public ChoiceService(IRepository<Choice> repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<Choice> Add(AddChoiceDto choiceDto)
    {
        var choice = _mapper.Map<Choice>(choiceDto);
        await _repository.Add(choice);
        return choice;
    }

    public async Task<ICollection<Choice>> AddRange(ICollection<AddChoiceDto> choiceDtos)
    {
        var choices = _mapper.Map<ICollection<Choice>>(choiceDtos);
        await _repository.AddRange(choices);
        return choices;
    }
}
