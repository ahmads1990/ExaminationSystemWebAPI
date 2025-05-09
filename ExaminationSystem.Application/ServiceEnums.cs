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