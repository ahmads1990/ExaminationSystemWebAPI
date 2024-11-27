namespace ExaminationSystemWebAPI.Helpers;

public static class CustomClaimTypes
{
    public const string ISADMIN = "isAdmin";
    public const string ISINSTRUCTOR = "isInstructor";
    public const string ISSTUDENT = "isStudent";
    public static readonly List<string> ALLOWEDTYPES = new List<string> { ISADMIN, ISINSTRUCTOR, ISSTUDENT };
}
