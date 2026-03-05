using ExaminationSystem.Application.Common.Attributes;

namespace ExaminationSystem.API.Models.Responses;

public enum ApiErrorCode
{
    [ErrorMessage("Success")]
    None = 0,
    
    // Authentication & Authorization (1000-1999)
    [ErrorMessage("Invalid username or password")]
    InvalidCredentials = 1001,
    
    [ErrorMessage("Please verify your email before logging in")]
    EmailNotVerified = 1002,
    
    [ErrorMessage("This email is already registered")]
    EmailAlreadyExists = 1003,
    
    [ErrorMessage("This username is already taken")]
    UsernameAlreadyExists = 1004,
    
    [ErrorMessage("Invalid or expired verification token")]
    InvalidVerificationToken = 1005,
    
    [ErrorMessage("Your session has expired. Please login again")]
    ExpiredToken = 1006,
    
    [ErrorMessage("You must be logged in to access this resource")]
    Unauthorized = 1007,
    
    [ErrorMessage("You don't have permission to access this resource")]
    Forbidden = 1008,

    [ErrorMessage("Invalid token")]
    InvalidToken = 1009,

    [ErrorMessage("Your exam time has expired")]
    ExamTimeout = 1010,

    // Validation Errors (2000-2999)
    [ErrorMessage("Validation failed")]
    ValidationFailed = 2000,
    
    // Resource Errors (3000-3999)
    [ErrorMessage("Resource not found")]
    ResourceNotFound = 3000,
    
    [ErrorMessage("Course not found")]
    CourseNotFound = 3001,
    
    [ErrorMessage("Exam not found")]
    ExamNotFound = 3002,
    
    [ErrorMessage("Question not found")]
    QuestionNotFound = 3003,
    
    [ErrorMessage("Student not found")]
    StudentNotFound = 3004,
    
    [ErrorMessage("Instructor not found")]
    InstructorNotFound = 3005,
    
    // Business Logic Errors (4000-4999)
    [ErrorMessage("You are already enrolled in this course")]
    AlreadyEnrolled = 4001,
    
    [ErrorMessage("This exam is not yet published")]
    ExamNotPublished = 4002,
    
    [ErrorMessage("The deadline for this exam has passed")]
    ExamDeadlinePassed = 4003,
    
    [ErrorMessage("You don't have permission to perform this action")]
    InsufficientPermissions = 4004,
    
    [ErrorMessage("Cannot delete course with enrolled students")]
    CannotDeleteCourseWithStudents = 4005,
    
    [ErrorMessage("Cannot delete a published exam")]
    CannotDeletePublishedExam = 4006,
    
    [ErrorMessage("Cannot unenroll from this course")]
    CannotUnenrollFromCourse = 4007,

    [ErrorMessage("This exam is already published")]
    ExamAlreadyPublished = 4008,

    [ErrorMessage("This exam is already unpublished")]
    ExamAlreadyUnpublished = 4009,

    [ErrorMessage("Cannot modify an archived exam")]
    ExamArchived = 4010,

    [ErrorMessage("Cannot publish an exam with no questions")]
    ExamHasNoQuestions = 4011,

    [ErrorMessage("Cannot unpublish an exam that has student submissions")]
    ExamHasSubmissions = 4012,

    [ErrorMessage("Cannot modify questions on a published exam")]
    ExamIsPublished = 4013,

    [ErrorMessage("This question is locked because it belongs to an exam with active submissions")]
    QuestionLocked = 4014,


    [ErrorMessage("You are not enrolled in this exam's course")]
    NotEnrolledInCourse = 4015,

    [ErrorMessage("You have reached the maximum number of attempts for this exam")]
    MaxAttemptsExceeded = 4016,

    [ErrorMessage("This exam attempt has already been completed")]
    AttemptAlreadyCompleted = 4017,

    [ErrorMessage("You already have an active attempt for this exam")]
    HasActiveAttempt = 4020,

    // Server Errors (5000-5999)
    [ErrorMessage("An internal server error occurred")]
    InternalServerError = 5000,
    
    [ErrorMessage("A database error occurred")]
    DatabaseError = 5001,
    
    [ErrorMessage("Failed to send email")]
    EmailServiceError = 5002,
    
    [ErrorMessage("Cache service error")]
    CacheError = 5003
}
