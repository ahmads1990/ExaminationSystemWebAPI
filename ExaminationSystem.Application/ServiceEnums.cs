public enum UserOperationResult
{
    Success,
    SuccessCheckMail,

    // Validation / Data issues
    ValidationFailed,
    EmailDuplicated,

    // User lookup / existence
    UserNotFound,
    InvalidUserId,
    InvalidCredentials,
    EmailNotConfirmed,

    // System / process issues
    UserCreationFailed,
    TokenGenerationFailed,

    // Fallback
    UnknownError
}

public enum UserEmailVerificationResult
{
    Success,
    EmailJobSent,
    TokenExpired,
    InvalidToken,
    UserNotFound,
    AlreadyConfirmed,
    UnknownError
}

public enum EmailTemplate
{
    Welcome
}