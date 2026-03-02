using ExaminationSystem.API.Models.Responses;

namespace ExaminationSystem.API.Extensions;

public static class ServiceResultExtensions
{
    /// <summary>
    /// Converts UserOperationResult to ApiErrorCode
    /// </summary>
    public static ApiErrorCode ToApiErrorCode(this UserOperationResult result)
    {
        return result switch
        {
            UserOperationResult.Success => ApiErrorCode.None,
            UserOperationResult.EmailDuplicated => ApiErrorCode.EmailAlreadyExists,
            UserOperationResult.InvalidCredentials => ApiErrorCode.InvalidCredentials,
            UserOperationResult.EmailNotConfirmed => ApiErrorCode.EmailNotVerified,
            UserOperationResult.ValidationFailed => ApiErrorCode.ValidationFailed,
            UserOperationResult.InvalidUserId => ApiErrorCode.ResourceNotFound,
            UserOperationResult.UserNotFound => ApiErrorCode.ResourceNotFound,
            UserOperationResult.TokenGenerationFailed => ApiErrorCode.InternalServerError,
            UserOperationResult.UserCreationFailed => ApiErrorCode.InternalServerError,
            UserOperationResult.InvalidRefreshToken => ApiErrorCode.InvalidCredentials,
            UserOperationResult.ExpiredRefreshToken => ApiErrorCode.ExpiredToken,
            _ => ApiErrorCode.InternalServerError
        };
    }
    
    /// <summary>
    /// Converts UserEmailVerificationResult to ApiErrorCode
    /// </summary>
    public static ApiErrorCode ToApiErrorCode(this UserEmailVerificationResult result)
    {
        return result switch
        {
            UserEmailVerificationResult.Success => ApiErrorCode.None,
            UserEmailVerificationResult.EmailJobSent => ApiErrorCode.None,
            UserEmailVerificationResult.InvalidToken => ApiErrorCode.InvalidVerificationToken,
            UserEmailVerificationResult.TokenExpired => ApiErrorCode.ExpiredToken,
            UserEmailVerificationResult.UserNotFound => ApiErrorCode.ResourceNotFound,
            UserEmailVerificationResult.AlreadyConfirmed => ApiErrorCode.None,
            _ => ApiErrorCode.InternalServerError
        };
    }
    
    /// <summary>
    /// Converts CourseOperationResult to ApiErrorCode
    /// </summary>
    public static ApiErrorCode ToApiErrorCode(this CourseOperationResult result)
    {
        return result switch
        {
            CourseOperationResult.Success => ApiErrorCode.None,
            CourseOperationResult.NotFound => ApiErrorCode.CourseNotFound,
            CourseOperationResult.MaxCoursesExceeded => ApiErrorCode.InsufficientPermissions,
            CourseOperationResult.DuplicateTitle => ApiErrorCode.ValidationFailed,
            CourseOperationResult.ValidationFailed => ApiErrorCode.ValidationFailed,
            CourseOperationResult.NotOwner => ApiErrorCode.Forbidden,
            _ => ApiErrorCode.InternalServerError
        };
    }
    
    /// <summary>
    /// Converts StudentCourseOperationResult to ApiErrorCode
    /// </summary>
    public static ApiErrorCode ToApiErrorCode(this StudentCourseOperationResult result)
    {
        return result switch
        {
            StudentCourseOperationResult.Success => ApiErrorCode.None,
            StudentCourseOperationResult.CourseNotFound => ApiErrorCode.CourseNotFound,
            StudentCourseOperationResult.AlreadyEnrolled => ApiErrorCode.AlreadyEnrolled,
            StudentCourseOperationResult.EnrollmentClosed => ApiErrorCode.ExamDeadlinePassed,
            StudentCourseOperationResult.MaxEnrollmentsExceeded => ApiErrorCode.InsufficientPermissions,
            StudentCourseOperationResult.ValidationFailed => ApiErrorCode.ValidationFailed,
            _ => ApiErrorCode.InternalServerError
        };
    }

    /// <summary>
    /// Converts ExamOperationResult to ApiErrorCode.
    /// </summary>
    public static ApiErrorCode ToApiErrorCode(this ExamOperationResult result)
    {
        return result switch
        {
            ExamOperationResult.Success => ApiErrorCode.None,
            ExamOperationResult.NotFound => ApiErrorCode.ExamNotFound,
            ExamOperationResult.AlreadyPublished => ApiErrorCode.ExamAlreadyPublished,
            ExamOperationResult.AlreadyUnpublished => ApiErrorCode.ExamAlreadyUnpublished,
            ExamOperationResult.ExamArchived => ApiErrorCode.ExamArchived,
            ExamOperationResult.NoQuestions => ApiErrorCode.ExamHasNoQuestions,
            ExamOperationResult.HasSubmissions => ApiErrorCode.ExamHasSubmissions,
            ExamOperationResult.ExamPublished => ApiErrorCode.ExamIsPublished,
            _ => ApiErrorCode.InternalServerError
        };
    }

    /// <summary>
    /// Converts QuestionOperationResult to ApiErrorCode.
    /// </summary>
    public static ApiErrorCode ToApiErrorCode(this QuestionOperationResult result)
    {
        return result switch
        {
            QuestionOperationResult.Success => ApiErrorCode.None,
            QuestionOperationResult.NotFound => ApiErrorCode.QuestionNotFound,
            QuestionOperationResult.Locked => ApiErrorCode.QuestionLocked,
            QuestionOperationResult.ValidationFailed => ApiErrorCode.ValidationFailed,
            _ => ApiErrorCode.InternalServerError
        };
    }
}
