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