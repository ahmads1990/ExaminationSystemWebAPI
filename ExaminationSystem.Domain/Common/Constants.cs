namespace ExaminationSystem.Domain.Common;

public static class Constants
{
    public const int MinChoicesCount = 2;
    public const int MaxChoicesCount = 5;
    public const int MaxAllowedCoursesPerInstructor = 3;
    public const int MaxAllowedEnrolledCoursesPerStudent = 5;

    // Exam rules
    public const int MinDurationInMinutes = 10;
    public const int MaxDurationInMinutes = 120;
    public const int MinTotalGrade = 10;
    public const int MaxTotalGrade = 150;
    public const int MinAttempts = 1;
    public const int MaxAttempts = 5;
    public const int MaxDeadlineYears = 1;

    // Question rules
    public const int MinQuestionBodyLength = 5;
    public const int MaxQuestionBodyLength = 100;
    public const int MinQuestionScore = 1;
    public const int MaxQuestionScore = 10;
    public const int MaxChoiceBodyLength = 200;

    // System
    public const string DBConnectionStringName = "DefaultConnection";
}

