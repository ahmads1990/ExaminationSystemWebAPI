public enum AddUserResult
{
    Success,
    ValidationFailed,
    EmailDuplicated
}

public enum AddInstructorResult
{
    Success,
    AlreadyInstructor,
    UserNotFound,
    InvalidUserId
}

public enum RegisterResult
{
    Success,
    ValidationFailed,
    EmailDuplicated,
    AlreadyRegistered,
    UserCreationFailed,
    TokenGenerationFailed,
    UnknownError
}

public enum EmailTemplate
{
    Welcome
}