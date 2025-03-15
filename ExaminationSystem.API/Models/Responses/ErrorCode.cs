namespace ExaminationSystem.API.Models.Responses;

public enum ErrorCode
{
    None = 0,
    UnKnownError,
    ValidationError,
    EntityNotFound,
    CannotDelete
}
