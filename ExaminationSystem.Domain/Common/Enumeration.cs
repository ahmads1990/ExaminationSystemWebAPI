using System.Text.Json.Serialization;

public enum SortingDirection
{
    [JsonPropertyName("asc")]
    Ascending,

    [JsonPropertyName("desc")]
    Descending
}

public enum QuestionLevel
{
    Easy = 0,
    Medium,
    Hard
}

public enum ExamType
{
    Quiz = 0,
    Final
};

public enum UserRole
{
    Student = 0,
    Instructor,
    Admin
}

public enum ExamStatus
{
    Draft = 0,
    Published,
    Archived
}

public enum ExamAttemptStatus
{
    NotStarted = 0,
    InProgress,
    Completed,
    TimedOut
}

public enum RejectionReason
{
    NotFound,
    AlreadyAssigned,
    NotAssigned
}