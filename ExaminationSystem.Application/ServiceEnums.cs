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
    InvalidRefreshToken,
    ExpiredRefreshToken,

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

public enum CourseOperationResult
{
    Success,
    NotFound,
    MaxCoursesExceeded,
    DuplicateTitle, 
    ValidationFailed,
    NotOwner
}

public enum StudentCourseOperationResult
{
    Success,
    CourseNotFound,
    AlreadyEnrolled,
    EnrollmentClosed,
    MaxEnrollmentsExceeded,
    ValidationFailed,
    UnknownError
}