using ExaminationSystem.API.Models.Requests.Questions;
using ExaminationSystem.API.Models.Responses;
using ExaminationSystem.Application.DTOs.Questions;
using Microsoft.AspNetCore.Mvc;

namespace ExaminationSystem.API.Controllers;

public class ExamsController : BaseController
{
    public ExamsController()
    {

    }

    [HttpGet]
    public async Task<PaginatedResponse<QuestionDto>> List(int pageIndex, int pageSize,
  /*  string? orderBy,*/ string? sortDirection, string? body, CancellationToken cancellationToken = default)
    {
        var sortingDirection = sortDirection == "desc" ? SortingDirection.Descending : SortingDirection.Ascending;
        var (questions, totalCount) = await _questionService.GetAll(pageIndex, pageSize, orderBy, sortingDirection, body);

        return new PaginatedResponse<QuestionDto>(questions, totalCount);
    }

    [HttpGet]
    public async Task<BaseResponse<QuestionDto?>> GetDetails(int id, CancellationToken cancellationToken = default)
    {
        var question = await _questionService.GetByID(id, cancellationToken);

        if (question is null)
            return new FailureResponse<QuestionDto?>(ErrorCode.EntityNotFound, "Couldnt find that entity");

        return new SuccessResponse<QuestionDto?>(question);
    }

    [HttpPost]
    public async Task<BaseResponse<QuestionDto>> Add(AddQuestionRequest request, CancellationToken cancellationToken = default)
    {
        var addQuestionDto = request.Adapt<AddQuestionDto>();
        var questionDto = await _questionService.Add(addQuestionDto, cancellationToken);

        return new SuccessResponse<QuestionDto>(questionDto);
    }

    [HttpPut]
    public async Task<BaseResponse<QuestionDto?>> Update(UpdateQuestionRequest request, CancellationToken cancellationToken = default)
    {
        var updateQuestionDto = request.Adapt<UpdateQuestionDto>();
        var questionDto = await _questionService.Update(updateQuestionDto, cancellationToken);

        if (questionDto is null)
        {
            return new FailureResponse<QuestionDto?>(ErrorCode.EntityNotFound, "Couldnt find that entity");
        }

        return new SuccessResponse<QuestionDto?>(questionDto);
    }

    [HttpDelete]
    public async Task<BaseResponse<object>> Delete(List<int> idsToDelete, CancellationToken cancellationToken = default)
    {
        var unDeletedIds = await _questionService.Delete(idsToDelete, cancellationToken);

        if (unDeletedIds is null || unDeletedIds.Any())
        {
            return new FailureResponse<object>(ErrorCode.CannotDelete, string.Join(',', idsToDelete));
        }

        return new SuccessResponse<object>("Deleted succuesfully");
    }
}
