using ExaminationSystemWebAPI.Models;
using ExaminationSystemWebAPI.Services.Interfaces;
using ExaminationSystemWebAPI.ViewModels.Choice;
using Mapster;
using Microsoft.AspNetCore.Mvc;

namespace ExaminationSystemWebAPI.Controllers;

[ApiController]
[Route("[controller]/[action]")]
public class ChoicesController : ControllerBase
{
    private readonly IChoiceService _choiceService;

    public ChoicesController(IChoiceService choiceService)
    {
        _choiceService = choiceService;
    }

    [HttpGet]
    public IActionResult GetAll()
    {
        var result = _choiceService
            .GetAll()
            .ToList();
        return Ok(result);
    }
    [HttpGet]
    public async Task<IActionResult> GetByID([FromQuery] string id)
    {
        var result = await _choiceService
            .GetByID(id);

        return Ok(result);
    }

    [HttpPost]
    public IActionResult Create(AddChoiceViewModel viewModel)
    {
        var choice = viewModel.Adapt<Choice>();

        _choiceService.Add(choice);

        _choiceService.SaveChanges();
        return Ok();
    }

    [HttpPatch]
    public IActionResult UpdateBody(UpdateChoiceBodyViewModel viewModel)
    {
        var choice = viewModel.Adapt<Choice>();

        _choiceService.UpdateTextBody(choice);

        _choiceService.SaveChanges();
        return Ok();
    }

    [HttpDelete]
    public IActionResult Delete([FromQuery] string id)
    {
        var choice = new Choice { ID = id };

        _choiceService.Delete(choice);
        _choiceService.SaveChanges();
        return Ok();
    }
}
