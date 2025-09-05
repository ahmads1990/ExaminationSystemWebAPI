public enum UserOperationResult
{
    Success,

    // Validation / Data issues
    ValidationFailed,
    EmailDuplicated,

    // User lookup / existence
    UserNotFound,
    InvalidUserId,

    // System / process issues
    UserCreationFailed,
    TokenGenerationFailed,

    // Fallback
    UnknownError
}

public enum EmailTemplate
{
    Welcome
}