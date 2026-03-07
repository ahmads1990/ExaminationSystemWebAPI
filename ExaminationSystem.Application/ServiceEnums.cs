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

public enum ExamOperationResult
{
    NotFound,
    Success,
    AlreadyPublished,
    AlreadyUnpublished,
    ExamArchived,
    ExamPublished,
    NoQuestions,
    HasSubmissions,
    ScoresMismatch,
    NotOwner
}

public enum QuestionOperationResult
{
    Success,
    NotFound,
    Locked,
    ValidationFailed
}

public enum StudentExamAttemptResult
{
    Success,
    ExamNotFound,
    StudentNotFound,
    ExamNotPublished,
    ExamDeadlinePassed,
    MaxAttemptsExceeded,
    NotEnrolled,
    HasActiveAttempt,
    AttemptAlreadyCompleted,
    GradingInProgress,
    AttemptNotCompleted,
    UnknownError
}
